using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Teachers.DTOs;
using SMS.Application.Features.Teachers.Queries;

namespace SMS.Application.Features.Teachers.Handlers.QueryHandlers;

/// <summary>
/// Handler for GetTeacherByIdQuery
/// </summary>
public class GetTeacherByIdQueryHandler : IRequestHandler<GetTeacherByIdQuery, TeacherDto?>
{
    private readonly IApplicationDbContext _context;

    public GetTeacherByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TeacherDto?> Handle(GetTeacherByIdQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var teacherId))
            return null;

        var teacher = await _context.Teachers
            .FirstOrDefaultAsync(x => x.Id == teacherId, cancellationToken);

        return teacher == null ? null : MapToDto(teacher);
    }

    private static TeacherDto MapToDto(SMS.Domain.Entities.Teacher teacher)
    {
        return new TeacherDto
        {
            Id = teacher.Id.ToString(),
            UserId = teacher.UserId.ToString(),
            FirstName = teacher.FirstName,
            LastName = teacher.LastName,
            Email = teacher.Email,
            Phone = teacher.Phone,
            Qualification = teacher.Qualification,
            ExperienceYears = teacher.ExperienceYears,
            JoiningDate = teacher.JoiningDate.ToDateTime(TimeOnly.MinValue),
            IsActive = teacher.IsActive,
            CreatedAt = teacher.CreatedAt,
            UpdatedAt = teacher.UpdatedAt,
            ImagePath = teacher.ImagePath
        };
    }
}

/// <summary>
/// Handler for GetAllTeachersQuery
/// </summary>
public class GetAllTeachersQueryHandler : IRequestHandler<GetAllTeachersQuery, PaginatedTeacherListDto>
{
    private readonly IApplicationDbContext _context;

    public GetAllTeachersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedTeacherListDto> Handle(GetAllTeachersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Teachers.AsQueryable();

        // Apply status filter
        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(x =>
                x.FirstName.ToLower().Contains(searchTerm) ||
                x.LastName.ToLower().Contains(searchTerm) ||
                x.Email.ToLower().Contains(searchTerm) ||
                (x.Phone != null && x.Phone.Contains(searchTerm)));
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var teachers = await query
            .OrderBy(x => x.FirstName)
            .ThenBy(x => x.LastName)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedTeacherListDto
        {
            Items = teachers.Select(MapToListDto).ToList(),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    private static TeacherListDto MapToListDto(SMS.Domain.Entities.Teacher teacher)
    {
        return new TeacherListDto
        {
            Id = teacher.Id.ToString(),
            FirstName = teacher.FirstName,
            LastName = teacher.LastName,
            Email = teacher.Email,
            Phone = teacher.Phone ?? string.Empty,
            Qualification = teacher.Qualification ?? string.Empty,
            ExperienceYears = teacher.ExperienceYears,
            JoiningDate = teacher.JoiningDate.ToDateTime(TimeOnly.MinValue),
            IsActive = teacher.IsActive,
            ImagePath = teacher.ImagePath
        };
    }
}

/// <summary>
/// Handler for GetTeacherByEmailQuery
/// </summary>
public class GetTeacherByEmailQueryHandler : IRequestHandler<GetTeacherByEmailQuery, TeacherDto?>
{
    private readonly IApplicationDbContext _context;

    public GetTeacherByEmailQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TeacherDto?> Handle(GetTeacherByEmailQuery request, CancellationToken cancellationToken)
    {
        var teacher = await _context.Teachers
            .FirstOrDefaultAsync(x => x.Email == request.Email, cancellationToken);

        return teacher == null ? null : MapToDto(teacher);
    }

    private static TeacherDto MapToDto(SMS.Domain.Entities.Teacher teacher)
    {
        return new TeacherDto
        {
            Id = teacher.Id.ToString(),
            UserId = teacher.UserId.ToString(),
            FirstName = teacher.FirstName,
            LastName = teacher.LastName,
            Email = teacher.Email,
            Phone = teacher.Phone,
            Qualification = teacher.Qualification,
            ExperienceYears = teacher.ExperienceYears,
            JoiningDate = teacher.JoiningDate.ToDateTime(TimeOnly.MinValue),
            IsActive = teacher.IsActive,
            CreatedAt = teacher.CreatedAt,
            UpdatedAt = teacher.UpdatedAt,
            ImagePath = teacher.ImagePath
        };
    }
}

/// <summary>
/// Handler for TeacherEmailExistsQuery
/// </summary>
public class TeacherEmailExistsQueryHandler : IRequestHandler<TeacherEmailExistsQuery, bool>
{
    private readonly IApplicationDbContext _context;

    public TeacherEmailExistsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(TeacherEmailExistsQuery request, CancellationToken cancellationToken)
    {
        var excludeTeacherId = string.IsNullOrEmpty(request.ExcludeTeacherId)
            ? null
            : Guid.TryParse(request.ExcludeTeacherId, out var id) ? (Guid?)id : null;

        var exists = await _context.Teachers
            .Where(x => x.Email == request.Email &&
                        (excludeTeacherId == null || x.Id != excludeTeacherId))
            .AnyAsync(cancellationToken);

        return exists;
    }
}

/// <summary>
/// Handler for GetActiveTeachersQuery
/// </summary>
public class GetActiveTeachersQueryHandler : IRequestHandler<GetActiveTeachersQuery, List<TeacherListDto>>
{
    private readonly IApplicationDbContext _context;

    public GetActiveTeachersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TeacherListDto>> Handle(GetActiveTeachersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Teachers.Where(x => x.IsActive);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(x =>
                x.FirstName.ToLower().Contains(searchTerm) ||
                x.LastName.ToLower().Contains(searchTerm) ||
                x.Email.ToLower().Contains(searchTerm));
        }

        var teachers = await query
            .OrderBy(x => x.FirstName)
            .ThenBy(x => x.LastName)
            .ToListAsync(cancellationToken);

        return teachers.Select(MapToListDto).ToList();
    }

    private static TeacherListDto MapToListDto(SMS.Domain.Entities.Teacher teacher)
    {
        return new TeacherListDto
        {
            Id = teacher.Id.ToString(),
            FirstName = teacher.FirstName,
            LastName = teacher.LastName,
            Email = teacher.Email,
            Phone = teacher.Phone ?? string.Empty,
            Qualification = teacher.Qualification ?? string.Empty,
            ExperienceYears = teacher.ExperienceYears,
            JoiningDate = teacher.JoiningDate.ToDateTime(TimeOnly.MinValue),
            IsActive = teacher.IsActive,
            ImagePath = teacher.ImagePath
        };
    }
}
