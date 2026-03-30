using SMS.Domain.Entities;

namespace SMS.Application.Features.StaffManagement.DTOs;

/// <summary>
/// DTO for teacher assignment details
/// </summary>
public class StaffAssignmentDto
{
    public Guid Id { get; set; }
    public Guid StaffId { get; set; }
    public Guid ClassId { get; set; }
    public Guid SectionId { get; set; }
    public Guid SubjectId { get; set; }
    public DateOnly AssignmentDate { get; set; }
    public DateOnly? RemovalDate { get; set; }
    
    // Additional display properties
    public string? ClassName { get; set; }
    public string? SectionName { get; set; }
    public string? SubjectName { get; set; }
    public string? SubjectCode { get; set; }
    public string? StaffName { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// DTO for creating a teacher assignment
/// </summary>
public class CreateStaffAssignmentDto
{
    public Guid ClassId { get; set; }
    public Guid SectionId { get; set; }
    public Guid SubjectId { get; set; }
    public DateOnly? AssignmentDate { get; set; }
}

/// <summary>
/// DTO for removing a teacher assignment
/// </summary>
public class RemoveStaffAssignmentDto
{
    public Guid AssignmentId { get; set; }
    public DateOnly? RemovalDate { get; set; }
}
