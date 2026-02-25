using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Classes.DTOs;
using SMS.Application.Features.Classes.Queries;

namespace SMS.Application.Features.Classes.Handlers.QueryHandlers;

/// <summary>
/// Handler for getting all classes with pagination
/// </summary>
public class GetAllClassesQueryHandler : IRequestHandler<GetAllClassesQuery, PaginatedClassListDto>
{
    private readonly IApplicationDbContext _context;

    public GetAllClassesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedClassListDto> Handle(GetAllClassesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Classes.AsQueryable();

        // Apply search filter
        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            query = query.Where(c => c.Name.Contains(request.SearchTerm) || 
                                     (c.AcademicYear != null && c.AcademicYear.Contains(request.SearchTerm)));
        }

        // Apply active filter
        if (request.IsActive.HasValue)
        {
            query = query.Where(c => c.IsActive == request.IsActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var classes = await query
            .Include(c => c.Sections)
            .OrderByDescending(c => c.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new ClassListDto
            {
                Id = c.Id.ToString(),
                Name = c.Name,
                AcademicYear = c.AcademicYear,
                IsActive = c.IsActive,
                SectionCount = c.Sections.Count
            })
            .ToListAsync(cancellationToken);

        return new PaginatedClassListDto
        {
            Items = classes,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}

/// <summary>
/// Handler for getting a specific class by ID with all sections
/// </summary>
public class GetClassByIdQueryHandler : IRequestHandler<GetClassByIdQuery, ClassDto?>
{
    private readonly IApplicationDbContext _context;

    public GetClassByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ClassDto?> Handle(GetClassByIdQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var classId))
            return null;

        var classEntity = await _context.Classes
            .Include(c => c.Sections)
                .ThenInclude(s => s.StudentSections)
            .FirstOrDefaultAsync(c => c.Id == classId, cancellationToken);

        if (classEntity == null)
            return null;

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
                StudentCount = s.StudentSections.Count(ss => ss.IsCurrent),
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            }).ToList(),
            CreatedAt = classEntity.CreatedAt,
            UpdatedAt = classEntity.UpdatedAt
        };
    }
}

/// <summary>
/// Handler for getting all sections for a specific class
/// </summary>
public class GetSectionsByClassIdQueryHandler : IRequestHandler<GetSectionsByClassIdQuery, List<SectionListDto>>
{
    private readonly IApplicationDbContext _context;

    public GetSectionsByClassIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SectionListDto>> Handle(GetSectionsByClassIdQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.ClassId, out var classId))
            return new List<SectionListDto>();

        var sections = await _context.Sections
            .Where(s => s.ClassId == classId)
            .OrderBy(s => s.SectionName)
            .Select(s => new SectionListDto
            {
                Id = s.Id.ToString(),
                SectionName = s.SectionName,
                IsActive = s.IsActive,
                StudentCount = s.StudentSections.Count(ss => ss.IsCurrent)
            })
            .ToListAsync(cancellationToken);

        return sections;
    }
}

/// <summary>
/// Handler for getting section details by ID
/// </summary>
public class GetSectionByIdQueryHandler : IRequestHandler<GetSectionByIdQuery, SectionDto?>
{
    private readonly IApplicationDbContext _context;

    public GetSectionByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SectionDto?> Handle(GetSectionByIdQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var sectionId))
            return null;

        var section = await _context.Sections
            .FirstOrDefaultAsync(s => s.Id == sectionId, cancellationToken);

        if (section == null)
            return null;

        // Get student count
        var studentCount = await _context.StudentSections
            .CountAsync(ss => ss.SectionId == sectionId && ss.IsCurrent, cancellationToken);

        return new SectionDto
        {
            Id = section.Id.ToString(),
            ClassId = section.ClassId.ToString(),
            SectionName = section.SectionName,
            IsActive = section.IsActive,
            StudentCount = studentCount,
            CreatedAt = section.CreatedAt,
            UpdatedAt = section.UpdatedAt
        };
    }
}

/// <summary>
/// Handler for getting student section history
/// </summary>
public class GetStudentSectionHistoryQueryHandler : IRequestHandler<GetStudentSectionHistoryQuery, StudentSectionHistoryDto>
{
    private readonly IApplicationDbContext _context;

    public GetStudentSectionHistoryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StudentSectionHistoryDto> Handle(GetStudentSectionHistoryQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.StudentId, out var studentId))
            return new StudentSectionHistoryDto { Items = new(), TotalCount = 0 };

        var studentSections = await _context.StudentSections
            .Where(ss => ss.StudentId == studentId)
            .Include(ss => ss.Section)
            .ThenInclude(s => s.Class)
            .Include(ss => ss.Student)
            .OrderByDescending(ss => ss.JoinedDate)
            .Select(ss => new StudentSectionDto
            {
                Id = ss.Id.ToString(),
                StudentId = ss.StudentId.ToString(),
                StudentName = ss.Student != null ? $"{ss.Student.FirstName} {ss.Student.LastName}" : "",
                SectionId = ss.SectionId.ToString(),
                SectionName = ss.Section != null ? ss.Section.SectionName : "",
                ClassName = ss.Section != null && ss.Section.Class != null ? ss.Section.Class.Name : "",
                JoinedDate = ss.JoinedDate,
                LeftDate = ss.LeftDate,
                IsCurrent = ss.IsCurrent,
                RollNumber = ss.RollNumber
            })
            .ToListAsync(cancellationToken);

        return new StudentSectionHistoryDto
        {
            Items = studentSections,
            TotalCount = studentSections.Count
        };
    }
}

/// <summary>
/// Handler for getting current section for a student
/// </summary>
public class GetStudentCurrentSectionQueryHandler : IRequestHandler<GetStudentCurrentSectionQuery, StudentSectionDto?>
{
    private readonly IApplicationDbContext _context;

    public GetStudentCurrentSectionQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StudentSectionDto?> Handle(GetStudentCurrentSectionQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.StudentId, out var studentId))
            return null;

        var studentSection = await _context.StudentSections
            .Where(ss => ss.StudentId == studentId && ss.IsCurrent)
            .Include(ss => ss.Section)
            .ThenInclude(s => s.Class)
            .Include(ss => ss.Student)
            .FirstOrDefaultAsync(cancellationToken);

        if (studentSection == null)
            return null;

        return new StudentSectionDto
        {
            Id = studentSection.Id.ToString(),
            StudentId = studentSection.StudentId.ToString(),
            StudentName = studentSection.Student != null ? $"{studentSection.Student.FirstName} {studentSection.Student.LastName}" : "",
            SectionId = studentSection.SectionId.ToString(),
            SectionName = studentSection.Section != null ? studentSection.Section.SectionName : "",
            ClassName = studentSection.Section != null && studentSection.Section.Class != null ? studentSection.Section.Class.Name : "",
            JoinedDate = studentSection.JoinedDate,
            LeftDate = studentSection.LeftDate,
            IsCurrent = studentSection.IsCurrent,
            RollNumber = studentSection.RollNumber
        };
    }
}

/// <summary>
/// Handler for getting all students with roll numbers in a section
/// </summary>
public class GetStudentsWithRollNumbersQueryHandler : IRequestHandler<GetStudentsWithRollNumbersQuery, List<StudentSectionDto>>
{
    private readonly IApplicationDbContext _context;

    public GetStudentsWithRollNumbersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<StudentSectionDto>> Handle(GetStudentsWithRollNumbersQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.SectionId, out var sectionId))
            return new();

        var studentSections = await _context.StudentSections
            .Where(ss => ss.SectionId == sectionId && ss.IsCurrent)
            .Include(ss => ss.Section)
            .ThenInclude(s => s.Class)
            .Include(ss => ss.Student)
            .OrderBy(ss => ss.RollNumber ?? int.MaxValue)
            .ThenBy(ss => ss.JoinedDate)
            .Select(ss => new StudentSectionDto
            {
                Id = ss.Id.ToString(),
                StudentId = ss.StudentId.ToString(),
                StudentName = ss.Student != null ? $"{ss.Student.FirstName} {ss.Student.LastName}" : "",
                SectionId = ss.SectionId.ToString(),
                SectionName = ss.Section != null ? ss.Section.SectionName : "",
                ClassName = ss.Section != null && ss.Section.Class != null ? ss.Section.Class.Name : "",
                JoinedDate = ss.JoinedDate,
                LeftDate = ss.LeftDate,
                IsCurrent = ss.IsCurrent,
                RollNumber = ss.RollNumber
            })
            .ToListAsync(cancellationToken);

        return studentSections;
    }
}
