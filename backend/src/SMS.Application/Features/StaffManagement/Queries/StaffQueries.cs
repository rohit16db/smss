using MediatR;
using SMS.Application.Features.StaffManagement.DTOs;
using SMS.Domain.Enums;

namespace SMS.Application.Features.StaffManagement.Queries;

/// <summary>
/// Query to get a staff member by ID
/// </summary>
public class GetStaffByIdQuery : IRequest<StaffDto?>
{
    public required string Id { get; set; }
}

/// <summary>
/// Query to get all staff with pagination
/// </summary>
public class GetAllStaffQuery : IRequest<PaginatedStaffListDto>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
    public Guid? DepartmentId { get; set; }
    public UserRole? RoleType { get; set; }
}

/// <summary>
/// Query to get a staff member by email
/// </summary>
public class GetStaffByEmailQuery : IRequest<StaffDto?>
{
    public required string Email { get; set; }
}

/// <summary>
/// Query to check if a staff member with email exists
/// </summary>
public class StaffEmailExistsQuery : IRequest<bool>
{
    public required string Email { get; set; }
    public string? ExcludeStaffId { get; set; }
}

/// <summary>
/// Query to get active staff only
/// </summary>
public class GetActiveStaffQuery : IRequest<List<StaffListDto>>
{
    public string? SearchTerm { get; set; }
    public UserRole? RoleType { get; set; }
}
