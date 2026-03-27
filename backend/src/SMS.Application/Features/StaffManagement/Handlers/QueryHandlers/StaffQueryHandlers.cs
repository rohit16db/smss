using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.StaffManagement.DTOs;
using SMS.Application.Features.StaffManagement.Queries;
using SMS.Domain.Entities;
using SMS.Domain.Enums;

namespace SMS.Application.Features.StaffManagement.Handlers.QueryHandlers;

/// <summary>
/// Handler for GetStaffByIdQuery
/// </summary>
public class GetStaffByIdQueryHandler : IRequestHandler<GetStaffByIdQuery, StaffDto?>
{
    private readonly IApplicationDbContext _context;

    public GetStaffByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StaffDto?> Handle(GetStaffByIdQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var staffId))
            return null;

        var staff = await _context.Staff
            .Include(x => x.UserProfile)
            .Include(x => x.Department)
            .Include(x => x.Qualifications)
            .FirstOrDefaultAsync(x => x.Id == staffId, cancellationToken);

        return staff == null ? null : MapToDto(staff);
    }

    private static StaffDto MapToDto(Staff staff)
    {
        return new StaffDto
        {
            Id = staff.Id.ToString(),
            UserId = staff.UserProfile.UserId.ToString(),
            FirstName = staff.UserProfile.FirstName,
            LastName = staff.UserProfile.LastName,
            Email = staff.UserProfile.Email,
            Phone = staff.UserProfile.Phone,
            Designation = staff.Designation,
            DepartmentId = staff.DepartmentId,
            DepartmentName = staff.Department?.Name ?? "Unknown",
            RoleType = staff.RoleType,
            ExperienceYears = staff.ExperienceYears,
            JoiningDate = staff.JoiningDate.ToDateTime(TimeOnly.MinValue),
            BasicSalary = staff.BasicSalary,
            IsActive = staff.IsActive,
            CreatedAt = staff.CreatedAt,
            UpdatedAt = staff.UpdatedAt,
            ImagePath = staff.UserProfile.ImagePath,
            Qualifications = staff.Qualifications.Select(q => new EducationalQualificationDto
            {
                Id = q.Id,
                Degree = q.DegreeName,
                Institution = q.Institution,
                YearOfPassing = q.YearOfPassing,
                GradeOrPercentage = q.GradeOrPercentage
            }).ToList()
        };
    }
}

/// <summary>
/// Handler for GetAllStaffQuery
/// </summary>
public class GetAllStaffQueryHandler : IRequestHandler<GetAllStaffQuery, PaginatedStaffListDto>
{
    private readonly IApplicationDbContext _context;

    public GetAllStaffQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedStaffListDto> Handle(GetAllStaffQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Staff
            .Include(x => x.UserProfile)
            .Include(x => x.Department)
            .AsQueryable();

        // Apply filters
        if (request.IsActive.HasValue)
            query = query.Where(x => x.IsActive == request.IsActive.Value);

        if (request.DepartmentId.HasValue)
            query = query.Where(x => x.DepartmentId == request.DepartmentId.Value);

        if (request.RoleType.HasValue)
            query = query.Where(x => x.RoleType == request.RoleType.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(x =>
                x.UserProfile.FirstName.ToLower().Contains(searchTerm) ||
                x.UserProfile.LastName.ToLower().Contains(searchTerm) ||
                x.UserProfile.Email.ToLower().Contains(searchTerm) ||
                (x.UserProfile.Phone != null && x.UserProfile.Phone.Contains(searchTerm)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.UserProfile.FirstName)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedStaffListDto
        {
            Items = items.Select(MapToListDto).ToList(),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    private static StaffListDto MapToListDto(Staff staff)
    {
        return new StaffListDto
        {
            Id = staff.Id.ToString(),
            FirstName = staff.UserProfile.FirstName,
            LastName = staff.UserProfile.LastName,
            Email = staff.UserProfile.Email,
            Phone = staff.UserProfile.Phone ?? string.Empty,
            Designation = staff.Designation,
            DepartmentName = staff.Department?.Name ?? "Unknown",
            RoleType = staff.RoleType,
            ExperienceYears = staff.ExperienceYears,
            JoiningDate = staff.JoiningDate.ToDateTime(TimeOnly.MinValue),
            IsActive = staff.IsActive,
            ImagePath = staff.UserProfile.ImagePath
        };
    }
}

/// <summary>
/// Handler for GetStaffByEmailQuery
/// </summary>
public class GetStaffByEmailQueryHandler : IRequestHandler<GetStaffByEmailQuery, StaffDto?>
{
    private readonly IApplicationDbContext _context;

    public GetStaffByEmailQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StaffDto?> Handle(GetStaffByEmailQuery request, CancellationToken cancellationToken)
    {
        var staff = await _context.Staff
            .Include(x => x.UserProfile)
            .Include(x => x.Department)
            .Include(x => x.Qualifications)
            .FirstOrDefaultAsync(x => x.UserProfile.Email == request.Email, cancellationToken);

        return staff == null ? null : MapToDto(staff);
    }

    private static StaffDto MapToDto(Staff staff)
    {
        // Reuse same mapping logic
        return new StaffDto
        {
            Id = staff.Id.ToString(),
            UserId = staff.UserProfile.UserId.ToString(),
            FirstName = staff.UserProfile.FirstName,
            LastName = staff.UserProfile.LastName,
            Email = staff.UserProfile.Email,
            Phone = staff.UserProfile.Phone,
            Designation = staff.Designation,
            DepartmentId = staff.DepartmentId,
            DepartmentName = staff.Department?.Name ?? "Unknown",
            RoleType = staff.RoleType,
            ExperienceYears = staff.ExperienceYears,
            JoiningDate = staff.JoiningDate.ToDateTime(TimeOnly.MinValue),
            BasicSalary = staff.BasicSalary,
            IsActive = staff.IsActive,
            CreatedAt = staff.CreatedAt,
            UpdatedAt = staff.UpdatedAt,
            ImagePath = staff.UserProfile.ImagePath,
            Qualifications = staff.Qualifications.Select(q => new EducationalQualificationDto
            {
                Id = q.Id,
                Degree = q.DegreeName,
                Institution = q.Institution,
                YearOfPassing = q.YearOfPassing,
                GradeOrPercentage = q.GradeOrPercentage
            }).ToList()
        };
    }
}

/// <summary>
/// Handler for StaffEmailExistsQuery
/// </summary>
public class StaffEmailExistsQueryHandler : IRequestHandler<StaffEmailExistsQuery, bool>
{
    private readonly IApplicationDbContext _context;

    public StaffEmailExistsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(StaffEmailExistsQuery request, CancellationToken cancellationToken)
    {
        var excludeStaffIdString = request.ExcludeStaffId;
        Guid? excludeId = null;
        if (!string.IsNullOrEmpty(excludeStaffIdString) && Guid.TryParse(excludeStaffIdString, out var guid))
        {
            excludeId = guid;
        }

        return await _context.Staff
            .AnyAsync(x => x.UserProfile.Email == request.Email && (excludeId == null || x.Id != excludeId), cancellationToken);
    }
}

/// <summary>
/// Handler for GetActiveStaffQuery
/// </summary>
public class GetActiveStaffQueryHandler : IRequestHandler<GetActiveStaffQuery, List<StaffListDto>>
{
    private readonly IApplicationDbContext _context;

    public GetActiveStaffQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<StaffListDto>> Handle(GetActiveStaffQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Staff
            .Include(x => x.UserProfile)
            .Include(x => x.Department)
            .Where(x => x.IsActive);

        if (request.RoleType.HasValue)
            query = query.Where(x => x.RoleType == request.RoleType.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(x =>
                x.UserProfile.FirstName.ToLower().Contains(searchTerm) ||
                x.UserProfile.LastName.ToLower().Contains(searchTerm));
        }

        var items = await query
            .OrderBy(x => x.UserProfile.FirstName)
            .ToListAsync(cancellationToken);

        return items.Select(MapToListDto).ToList();
    }

    private static StaffListDto MapToListDto(Staff staff)
    {
        return new StaffListDto
        {
            Id = staff.Id.ToString(),
            FirstName = staff.UserProfile.FirstName,
            LastName = staff.UserProfile.LastName,
            Email = staff.UserProfile.Email,
            Phone = staff.UserProfile.Phone ?? string.Empty,
            Designation = staff.Designation,
            DepartmentName = staff.Department?.Name ?? "Unknown",
            RoleType = staff.RoleType,
            ExperienceYears = staff.ExperienceYears,
            JoiningDate = staff.JoiningDate.ToDateTime(TimeOnly.MinValue),
            IsActive = staff.IsActive,
            ImagePath = staff.UserProfile.ImagePath
        };
    }
}
