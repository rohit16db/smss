using SMS.Domain.Entities;

namespace SMS.Application.Features.Teachers.DTOs;

/// <summary>
/// DTO for teacher assignment details
/// </summary>
public class TeacherAssignmentDto
{
    public Guid Id { get; set; }
    public Guid TeacherId { get; set; }
    public Guid ClassId { get; set; }
    public Guid SubjectId { get; set; }
    public DateOnly AssignmentDate { get; set; }
    public DateOnly? RemovalDate { get; set; }
    
    // Additional display properties
    public string? ClassName { get; set; }
    public string? SubjectName { get; set; }
    public string? SubjectCode { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// DTO for creating a teacher assignment
/// </summary>
public class CreateTeacherAssignmentDto
{
    public Guid ClassId { get; set; }
    public Guid SubjectId { get; set; }
    public DateOnly? AssignmentDate { get; set; }
}

/// <summary>
/// DTO for removing a teacher assignment
/// </summary>
public class RemoveTeacherAssignmentDto
{
    public Guid AssignmentId { get; set; }
    public DateOnly? RemovalDate { get; set; }
}
