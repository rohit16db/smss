# CQRS Handlers Implementation Template

## Overview
This file provides templates and patterns for implementing the remaining CQRS handlers (T016-T030) following Single Responsibility Principle.

---

## Template 1: Create Command Handler

```csharp
using MediatR;
using AutoMapper;
using SMS.Domain.Entities;
using SMS.Domain.Services;
using SMS.Infrastructure.Repositories;
using SMS.Application.DTOs; // To be created

namespace SMS.Application.Features.Exams.Commands;

/// <summary>
/// Command to create a new exam
/// Single Responsibility: Transfer exam creation request data
/// </summary>
public class CreateExamCommand : IRequest<ExamDto>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime ExamDate { get; set; }
    public decimal TotalMarks { get; set; } = 100;
    public decimal PassMarks { get; set; } = 40;
    public Guid CreatedById { get; set; }
    public List<Guid> SubjectIds { get; set; } = new();
    public List<Guid> ClassIds { get; set; } = new();
}

/// <summary>
/// Handler for CreateExamCommand
/// Single Responsibility: Orchestrate exam creation with validation
/// </summary>
public class CreateExamCommandHandler : IRequestHandler<CreateExamCommand, ExamDto>
{
    private readonly IExamRepository _examRepository;
    private readonly IMapper _mapper;

    public CreateExamCommandHandler(IExamRepository examRepository, IMapper mapper)
    {
        _examRepository = examRepository;
        _mapper = mapper;
    }

    public async Task<ExamDto> Handle(CreateExamCommand request, CancellationToken cancellationToken)
    {
        // 1. Validation: Exam date not in past (business rule)
        if (request.ExamDate < DateTime.UtcNow.Date)
            throw new InvalidOperationException("Exam date cannot be in the past");

        // 2. Validation: At least one subject and class
        if (!request.SubjectIds.Any() || !request.ClassIds.Any())
            throw new InvalidOperationException("Exam must have at least one subject and class");

        // 3. Create exam entity
        var exam = new Exam
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            ExamDate = request.ExamDate,
            TotalMarks = request.TotalMarks,
            PassMarks = request.PassMarks,
            CreatedById = request.CreatedById,
            Status = ExamStatus.Draft
        };

        // 4. Add exam subjects via database configuration
        // Note: ExamSubjects and ExamClasses should be added separately

        // 5. Save to database
        var createdExam = await _examRepository.CreateAsync(exam, cancellationToken);

        // 6. Return DTO
        return _mapper.Map<ExamDto>(createdExam);
    }
}
```

---

## Template 2: Update Command Handler

```csharp
public class UpdateExamCommand : IRequest<ExamDto>
{
    public Guid ExamId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    // Only allow changes to these fields if status is Draft
}

public class UpdateExamCommandHandler : IRequestHandler<UpdateExamCommand, ExamDto>
{
    private readonly IExamRepository _examRepository;
    private readonly IMapper _mapper;

    public UpdateExamCommandHandler(IExamRepository examRepository, IMapper mapper)
    {
        _examRepository = examRepository;
        _mapper = mapper;
    }

    public async Task<ExamDto> Handle(UpdateExamCommand request, CancellationToken cancellationToken)
    {
        // 1. Get existing exam
        var exam = await _examRepository.GetByIdAsync(request.ExamId, cancellationToken);
        if (exam == null)
            throw new KeyNotFoundException($"Exam {request.ExamId} not found");

        // 2. Validate: Only draft exams can be updated
        if (exam.Status != ExamStatus.Draft)
            throw new InvalidOperationException("Only draft exams can be updated");

        // 3. Update properties
        exam.Name = request.Name;
        exam.Description = request.Description;

        // 4. Save changes
        var updated = await _examRepository.UpdateAsync(exam, cancellationToken);

        // 5. Return DTO
        return _mapper.Map<ExamDto>(updated);
    }
}
```

---

## Template 3: Marks Entry Command Handler

```csharp
public class SaveStudentMarksCommand : IRequest<MarksSaveResultDto>
{
    public Guid ExamId { get; set; }
    public Guid ClassId { get; set; }
    public List<StudentMarkEntryDto> MarksData { get; set; } = new();
}

public class SaveStudentMarksCommandHandler : IRequestHandler<SaveStudentMarksCommand, MarksSaveResultDto>
{
    private readonly IStudentMarksRepository _marksRepository;
    private readonly IMarksValidationService _validationService;
    private readonly IExamRepository _examRepository;

    public SaveStudentMarksCommandHandler(
        IStudentMarksRepository marksRepository,
        IMarksValidationService validationService,
        IExamRepository examRepository)
    {
        _marksRepository = marksRepository;
        _validationService = validationService;
        _examRepository = examRepository;
    }

    public async Task<MarksSaveResultDto> Handle(SaveStudentMarksCommand request, CancellationToken cancellationToken)
    {
        // 1. Get exam and validate it's published
        var exam = await _examRepository.GetByIdAsync(request.ExamId, cancellationToken);
        if (exam == null)
            throw new KeyNotFoundException("Exam not found");
        if (exam.Status != ExamStatus.Published)
            throw new InvalidOperationException("Exam must be published for marks entry");

        // 2. Build StudentMarks entities from DTO
        var studentMarks = request.MarksData.Select(m => new StudentMarks
        {
            Id = Guid.NewGuid(),
            ExamId = request.ExamId,
            StudentId = m.StudentId,
            SubjectId = m.SubjectId,
            MarksObtained = m.MarksObtained,
            IsAbsent = m.IsAbsent
        }).ToList();

        // 3. Validate all marks using domain service
        var examSubjectMap = exam.ExamSubjects.ToDictionary(es => es.SubjectId);
        var validation = _validationService.ValidateAllStudentMarks(studentMarks, examSubjectMap);
        
        if (!validation.IsValid)
            throw new InvalidOperationException($"Marks validation failed: {validation.Message}");

        // 4. Save marks (service handles upsert logic)
        await _marksRepository.SaveAsync(studentMarks, cancellationToken);

        // 5. Return result
        return new MarksSaveResultDto
        {
            Success = true,
            Message = "Marks saved successfully",
            MarksCount = studentMarks.Count
        };
    }
}
```

---

## Template 4: Submit Marks Command Handler (Generates Report Cards)

```csharp
public class SubmitMarksCommand : IRequest<SubmitMarksResultDto>
{
    public Guid ExamId { get; set; }
    public Guid ClassId { get; set; }
    public Guid SubmittedById { get; set; }
}

public class SubmitMarksCommandHandler : IRequestHandler<SubmitMarksCommand, SubmitMarksResultDto>
{
    private readonly IExamRepository _examRepository;
    private readonly IStudentMarksRepository _marksRepository;
    private readonly IReportCardRepository _reportCardRepository;
    private readonly IReportCardGenerationService _reportCardService;
    private readonly IGradeConfigurationRepository _gradeRepository;

    // Constructor omitted for brevity...

    public async Task<SubmitMarksResultDto> Handle(SubmitMarksCommand request, CancellationToken cancellationToken)
    {
        // 1. Get exam and class
        var exam = await _examRepository.GetByIdAsync(request.ExamId, cancellationToken);
        var marks = await _marksRepository.GetByExamAndClassAsync(request.ExamId, request.ClassId, cancellationToken);

        // 2. Validate all students have marks or are marked absent
        var studentCount = await _getStudentCountInClass(request.ClassId, cancellationToken);
        if (marks.Count != studentCount)
            throw new InvalidOperationException("Not all students have marks entered");

        // 3. Get grade configurations
        var gradeConfigs = await _gradeRepository.GetBySchoolAsync(Guid.NewGuid(), cancellationToken);
        
        // 4. Generate report cards for each student
        var reportCardsCount = 0;
        foreach (var studentId in marks.Select(m => m.StudentId).Distinct())
        {
            var studentMarks = marks.Where(m => m.StudentId == studentId).ToList();
            var position = await _calculateClassPosition(studentId, marks, cancellationToken);
            
            var reportCard = await _reportCardService.GenerateReportCardAsync(
                exam,
                null, // Get student from database
                studentMarks,
                exam.ExamSubjects.ToList(),
                gradeConfigs,
                position,
                cancellationToken);

            await _reportCardRepository.CreateAsync(reportCard, cancellationToken);
            reportCardsCount++;
        }

        // 5. Update exam class status
        // TODO: Add UpdateExamClassStatus method to repository

        return new SubmitMarksResultDto
        {
            Success = true,
            Message = "Marks submitted successfully",
            ReportCardsGenerated = reportCardsCount
        };
    }
}
```

---

## Template 5: Query Handler

```csharp
public class GetExamsQuery : IRequest<List<ExamDto>>
{
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 20;
    public string? Status { get; set; }
}

public class GetExamsQueryHandler : IRequestHandler<GetExamsQuery, List<ExamDto>>
{
    private readonly IExamRepository _examRepository;
    private readonly IMapper _mapper;

    public GetExamsQueryHandler(IExamRepository examRepository, IMapper mapper)
    {
        _examRepository = examRepository;
        _mapper = mapper;
    }

    public async Task<List<ExamDto>> Handle(GetExamsQuery request, CancellationToken cancellationToken)
    {
        // 1. Get data from repository
        List<Exam> exams;
        if (!string.IsNullOrEmpty(request.Status))
        {
            exams = await _examRepository.GetByStatusAsync(request.Status, cancellationToken);
        }
        else
        {
            exams = await _examRepository.GetAllAsync(request.Skip, request.Take, cancellationToken);
        }

        // 2. Map to DTOs
        return _mapper.Map<List<ExamDto>>(exams);
    }
}
```

---

## Task Breakdown for T016-T030

### Commands (T016-T023) - 8 handlers
1. `CreateExamCommand` → `CreateExamCommandHandler`
2. `UpdateExamCommand` → `UpdateExamCommandHandler`
3. `PublishExamCommand` → `PublishExamCommandHandler`
4. `DeleteExamCommand` → `DeleteExamCommandHandler`
5. `SaveStudentMarksCommand` → `SaveStudentMarksCommandHandler`
6. `SubmitMarksCommand` → `SubmitMarksCommandHandler`
7. `GenerateReportCardCommand` → `GenerateReportCardCommandHandler`
8. `ConfigureGradesCommand` → `ConfigureGradesCommandHandler`

### Queries (T024-T030) - 7 handlers
1. `GetExamsQuery` → `GetExamsQueryHandler`
2. `GetExamByIdQuery` → `GetExamByIdQueryHandler`
3. `GetStudentMarksQuery` → `GetStudentMarksQueryHandler`
4. `GetSingleStudentMarksQuery` → `GetSingleStudentMarksQueryHandler`
5. `GetReportCardQuery` → `GetReportCardQueryHandler`
6. `GetExamReportCardsQuery` → `GetExamReportCardsQueryHandler`
7. `GetGradeConfigurationQuery` → `GetGradeConfigurationQueryHandler`

---

## SRP Principles in Handlers

Each handler should:
1. ✅ Handle ONE command/query
2. ✅ Orchestrate - don't do heavy lifting
3. ✅ Use repositories for data access
4. ✅ Use domain services for business logic
5. ✅ Use mapper for DTO conversion
6. ✅ Be easy to test with mock dependencies

✅ Not do:
- Directly access DbContext
- Mix business logic with orchestration
- Have >100 lines of code
- Do calculations without domain service

---

## Registration in DI Container

```csharp
// In Program.cs
builder.Services.AddScoped<IExamRepository, ExamRepository>();
builder.Services.AddScoped<IStudentMarksRepository, StudentMarksRepository>();
builder.Services.AddScoped<IReportCardRepository, ReportCardRepository>();
builder.Services.AddScoped<IGradeConfigurationRepository, GradeConfigurationRepository>();

builder.Services.AddScoped<IGradeCalculationService, GradeCalculationService>();
builder.Services.AddScoped<IMarksValidationService, MarksValidationService>();
builder.Services.AddScoped<IReportCardGenerationService, ReportCardGenerationService>();

builder.Services.AddAutoMapper(typeof(Program).Assembly);
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
```

---

## Next Files to Create

1. All Commands & Handlers (T016-T023)
2. All Queries & Handlers (T024-T030)
3. DTOs for each (T031-T034)
4. API Controllers (T035-T040)
5. Frontend (T041-T057)

Following this pattern will ensure code is maintainable, testable, and follows SRP!

