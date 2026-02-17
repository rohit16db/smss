using MediatR;
using SMS.Application.Features.Teachers.DTOs;

namespace SMS.Application.Features.Teachers.Commands;

/// <summary>
/// Command to create a new teacher assignment
/// </summary>
public class CreateTeacherAssignmentCommand : IRequest<TeacherAssignmentDto>
{
    public Guid TeacherId { get; set; }
    public Guid ClassId { get; set; }
    public Guid SubjectId { get; set; }
    public DateOnly? AssignmentDate { get; set; }
}

/// <summary>
/// Command to remove a teacher assignment
/// </summary>
public class RemoveTeacherAssignmentCommand : IRequest<bool>
{
    public Guid AssignmentId { get; set; }
    public DateOnly? RemovalDate { get; set; }
}
