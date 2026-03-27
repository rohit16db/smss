using SMS.Domain.Entities;
using SMS.Domain.Enums;

namespace SMS.Application.Features.StaffManagement.DTOs;

/// <summary>
/// DTO for creating a new staff member
/// </summary>
public class CreateStaffDto
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string Phone { get; set; }
    public required string Designation { get; set; }
    public Guid? DepartmentId { get; set; }
    public required UserRole RoleType { get; set; }
    public required int ExperienceYears { get; set; }
    public required DateTime JoiningDate { get; set; }
    public decimal BasicSalary { get; set; }
    public string? ImagePath { get; set; }
    public List<EducationalQualificationDto> Qualifications { get; set; } = new();
}

/// <summary>
/// DTO for updating staff information
/// </summary>
public class UpdateStaffDto
{
    public required string Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string Phone { get; set; }
    public required string Designation { get; set; }
    public Guid? DepartmentId { get; set; }
    public required UserRole RoleType { get; set; }
    public required int ExperienceYears { get; set; }
    public required bool IsActive { get; set; }
    public decimal BasicSalary { get; set; }
    public string? ImagePath { get; set; }
    public List<EducationalQualificationDto> Qualifications { get; set; } = new();
}

/// <summary>
/// DTO for reading staff information
/// </summary>
public class StaffDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Designation { get; set; } = string.Empty;
    public Guid? DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public UserRole RoleType { get; set; }
    public int ExperienceYears { get; set; }
    public DateTime JoiningDate { get; set; }
    public decimal BasicSalary { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? ImagePath { get; set; }
    public List<EducationalQualificationDto> Qualifications { get; set; } = new();
}

/// <summary>
/// DTO for staff list with pagination
/// </summary>
public class StaffListDto
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public UserRole RoleType { get; set; }
    public int ExperienceYears { get; set; }
    public DateTime JoiningDate { get; set; }
    public bool IsActive { get; set; }
    public string? ImagePath { get; set; }
}

/// <summary>
/// DTO for paginated staff list response
/// </summary>
public class PaginatedStaffListDto
{
    public List<StaffListDto> Items { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
}

public class EducationalQualificationDto
{
    public Guid Id { get; set; }
    public string Degree { get; set; } = string.Empty;
    public string Institution { get; set; } = string.Empty;
    public int YearOfPassing { get; set; }
    public string? GradeOrPercentage { get; set; }
}
