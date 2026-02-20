using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Classes.Commands;
using SMS.Application.Features.Classes.DTOs;
using SMS.Domain.Entities;

namespace SMS.Application.Features.Classes.Handlers.CommandHandlers;

/// <summary>
/// Handler for creating a new class
/// </summary>
public class CreateClassCommandHandler : IRequestHandler<CreateClassCommand, ClassDto>
{
    private readonly IApplicationDbContext _context;

    public CreateClassCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ClassDto> Handle(CreateClassCommand request, CancellationToken cancellationToken)
    {
        var classEntity = new Class
        {
            Name = request.Name,
            AcademicYear = request.AcademicYear,
            IsActive = true
        };

        _context.Classes.Add(classEntity);
        await _context.SaveChangesAsync(cancellationToken);

        return new ClassDto
        {
            Id = classEntity.Id.ToString(),
            Name = classEntity.Name,
            AcademicYear = classEntity.AcademicYear,
            IsActive = classEntity.IsActive,
            Sections = new(),
            CreatedAt = classEntity.CreatedAt,
            UpdatedAt = classEntity.UpdatedAt
        };
    }
}

/// <summary>
/// Handler for updating a class
/// </summary>
public class UpdateClassCommandHandler : IRequestHandler<UpdateClassCommand, ClassDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateClassCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ClassDto> Handle(UpdateClassCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var classId))
            throw new Exception("Invalid class ID");

        var classEntity = await _context.Classes
            .Include(c => c.Sections)
            .FirstOrDefaultAsync(c => c.Id == classId, cancellationToken);

        if (classEntity == null)
            throw new Exception($"Class with ID {request.Id} not found");

        classEntity.Name = request.Name;
        classEntity.AcademicYear = request.AcademicYear;
        classEntity.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        return new ClassDto
        {
            Id = classEntity.Id.ToString(),
            Name = classEntity.Name,
            AcademicYear = classEntity.AcademicYear,
            IsActive = classEntity.IsActive,
            Sections = classEntity.Sections.Select(s => new SectionDto
            {
                Id = s.Id.ToString(),
                ClassId = s.ClassId.ToString(),
                SectionName = s.SectionName,
                IsActive = s.IsActive,
                StudentCount = 0, // Will be populated from StudentSections if needed
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            }).ToList(),
            CreatedAt = classEntity.CreatedAt,
            UpdatedAt = classEntity.UpdatedAt
        };
    }
}

/// <summary>
/// Handler for deleting a class
/// </summary>
public class DeleteClassCommandHandler : IRequestHandler<DeleteClassCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteClassCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteClassCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var classId))
            throw new Exception("Invalid class ID");

        var classEntity = await _context.Classes.FirstOrDefaultAsync(c => c.Id == classId, cancellationToken);

        if (classEntity == null)
            throw new Exception($"Class with ID {request.Id} not found");

        _context.Classes.Remove(classEntity);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}

/// <summary>
/// Handler for creating a new section
/// </summary>
public class CreateSectionCommandHandler : IRequestHandler<CreateSectionCommand, SectionDto>
{
    private readonly IApplicationDbContext _context;

    public CreateSectionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SectionDto> Handle(CreateSectionCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.ClassId, out var classId))
            throw new Exception("Invalid class ID");

        var classEntity = await _context.Classes.FirstOrDefaultAsync(c => c.Id == classId, cancellationToken);
        if (classEntity == null)
            throw new Exception($"Class with ID {request.ClassId} not found");

        var section = new Section
        {
            ClassId = classId,
            SectionName = request.SectionName,
            IsActive = true
        };

        _context.Sections.Add(section);
        await _context.SaveChangesAsync(cancellationToken);

        return new SectionDto
        {
            Id = section.Id.ToString(),
            ClassId = section.ClassId.ToString(),
            SectionName = section.SectionName,
            IsActive = section.IsActive,
            StudentCount = 0,
            CreatedAt = section.CreatedAt,
            UpdatedAt = section.UpdatedAt
        };
    }
}

/// <summary>
/// Handler for updating a section
/// </summary>
public class UpdateSectionCommandHandler : IRequestHandler<UpdateSectionCommand, SectionDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateSectionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SectionDto> Handle(UpdateSectionCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var sectionId))
            throw new Exception("Invalid section ID");

        var section = await _context.Sections.FirstOrDefaultAsync(s => s.Id == sectionId, cancellationToken);

        if (section == null)
            throw new Exception($"Section with ID {request.Id} not found");

        section.SectionName = request.SectionName;
        section.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        return new SectionDto
        {
            Id = section.Id.ToString(),
            ClassId = section.ClassId.ToString(),
            SectionName = section.SectionName,
            IsActive = section.IsActive,
            StudentCount = 0,
            CreatedAt = section.CreatedAt,
            UpdatedAt = section.UpdatedAt
        };
    }
}

/// <summary>
/// Handler for deleting a section
/// </summary>
public class DeleteSectionCommandHandler : IRequestHandler<DeleteSectionCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteSectionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteSectionCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var sectionId))
            throw new Exception("Invalid section ID");

        var section = await _context.Sections.FirstOrDefaultAsync(s => s.Id == sectionId, cancellationToken);

        if (section == null)
            throw new Exception($"Section with ID {request.Id} not found");

        _context.Sections.Remove(section);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}

/// <summary>
/// Handler for moving a student to a different section
/// </summary>
public class MoveStudentSectionCommandHandler : IRequestHandler<MoveStudentSectionCommand, StudentSectionDto>
{
    private readonly IApplicationDbContext _context;

    public MoveStudentSectionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StudentSectionDto> Handle(MoveStudentSectionCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.StudentId, out var studentId))
            throw new Exception("Invalid student ID");

        if (!Guid.TryParse(request.NewSectionId, out var newSectionId))
            throw new Exception("Invalid section ID");

        var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == studentId, cancellationToken);
        if (student == null)
            throw new Exception($"Student with ID {request.StudentId} not found");

        var section = await _context.Sections
            .Include(s => s.Class)
            .FirstOrDefaultAsync(s => s.Id == newSectionId, cancellationToken);

        if (section == null)
            throw new Exception($"Section with ID {request.NewSectionId} not found");

        // Mark old current section as not current and set left date
        var oldCurrentSection = await _context.StudentSections
            .FirstOrDefaultAsync(ss => ss.StudentId == studentId && ss.IsCurrent, cancellationToken);

        if (oldCurrentSection != null)
        {
            oldCurrentSection.IsCurrent = false;
            oldCurrentSection.LeftDate = DateTime.UtcNow;
        }

        // Create new enrollment record
        var newStudentSection = new StudentSection
        {
            StudentId = studentId,
            SectionId = newSectionId,
            JoinedDate = DateTime.UtcNow,
            IsCurrent = true
        };

        _context.StudentSections.Add(newStudentSection);
        await _context.SaveChangesAsync(cancellationToken);

        return new StudentSectionDto
        {
            Id = newStudentSection.Id.ToString(),
            StudentId = newStudentSection.StudentId.ToString(),
            StudentName = $"{student.FirstName} {student.LastName}",
            SectionId = newStudentSection.SectionId.ToString(),
            SectionName = section.SectionName,
            ClassName = section.Class?.Name ?? "",
            JoinedDate = newStudentSection.JoinedDate,
            LeftDate = newStudentSection.LeftDate,
            IsCurrent = newStudentSection.IsCurrent,
            RollNumber = newStudentSection.RollNumber
        };
    }
}

/// <summary>
/// Handler for auto-assigning sequential roll numbers
/// </summary>
public class AutoAssignRollNumbersCommandHandler : IRequestHandler<AutoAssignRollNumbersCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IRollNumberService _rollNumberService;

    public AutoAssignRollNumbersCommandHandler(IApplicationDbContext context, IRollNumberService rollNumberService)
    {
        _context = context;
        _rollNumberService = rollNumberService;
    }

    public async Task<bool> Handle(AutoAssignRollNumbersCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.SectionId, out var sectionId))
            throw new ArgumentException("Invalid section ID");

        await _rollNumberService.AssignSequentialRollNumbersAsync(sectionId, cancellationToken);
        return true;
    }
}

/// <summary>
/// Handler for updating a student's roll number
/// </summary>
public class UpdateStudentRollNumberCommandHandler : IRequestHandler<UpdateStudentRollNumberCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IRollNumberService _rollNumberService;

    public UpdateStudentRollNumberCommandHandler(IApplicationDbContext context, IRollNumberService rollNumberService)
    {
        _context = context;
        _rollNumberService = rollNumberService;
    }

    public async Task<bool> Handle(UpdateStudentRollNumberCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.StudentSectionId, out var studentSectionId))
            throw new ArgumentException("Invalid student section ID");

        await _rollNumberService.UpdateRollNumberAsync(studentSectionId, request.RollNumber, cancellationToken);
        return true;
    }
}

/// <summary>
/// Handler for bulk updating roll numbers
/// </summary>
public class BulkUpdateRollNumbersCommandHandler : IRequestHandler<BulkUpdateRollNumbersCommand, bool>
{
    private readonly IRollNumberService _rollNumberService;

    public BulkUpdateRollNumbersCommandHandler(IRollNumberService rollNumberService)
    {
        _rollNumberService = rollNumberService;
    }

    public async Task<bool> Handle(BulkUpdateRollNumbersCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.SectionId, out var sectionId))
            throw new ArgumentException("Invalid section ID");

        var rollNumberUpdates = request.RollNumberUpdates
            .ToDictionary(
                kvp => Guid.Parse(kvp.Key),
                kvp => kvp.Value
            );

        await _rollNumberService.BulkUpdateRollNumbersAsync(sectionId, rollNumberUpdates, cancellationToken);
        return true;
    }
}
