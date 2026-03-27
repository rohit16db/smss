using MediatR;
using SMS.Application.Features.StaffManagement.DTOs;
using SMS.Domain.Enums;

namespace SMS.Application.Features.StaffManagement.Commands;

/// <summary>
/// Command to create a new staff member
/// </summary>
public class CreateStaffCommand : IRequest<StaffDto>
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
    public string? CreatedByUserId { get; set; }
}

/// <summary>
/// Command to update staff information
/// </summary>
public class UpdateStaffCommand : IRequest<StaffDto>
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
    public string? UpdatedByUserId { get; set; }
}

/// <summary>
/// Command to deactivate a staff member
/// </summary>
public class DeactivateStaffCommand : IRequest<bool>
{
    public required string Id { get; set; }
    public string? UpdatedByUserId { get; set; }
}

/// <summary>
/// Command to activate a staff member
/// </summary>
public class ActivateStaffCommand : IRequest<bool>
{
    public required string Id { get; set; }
    public string? UpdatedByUserId { get; set; }
}

/// <summary>
/// Command to delete a staff member
/// </summary>
public class DeleteStaffCommand : IRequest<bool>
{
    public required string Id { get; set; }
}
