using MediatR;
using SMS.Application.Features.StaffManagement.DTOs;

namespace SMS.Application.Features.StaffManagement.Commands;

/// <summary>
/// Command to create a new teacher assignment
/// </summary>
public class CreateStaffAssignmentCommand : IRequest<StaffAssignmentDto>
{
    public Guid StaffId { get; set; }
    public Guid ClassId { get; set; }
    public Guid SectionId { get; set; }
    public Guid SubjectId { get; set; }
    public DateOnly? AssignmentDate { get; set; }
}

/// <summary>
/// Command to remove a teacher assignment
/// </summary>
public class RemoveStaffAssignmentCommand : IRequest<bool>
{
    public Guid AssignmentId { get; set; }
    public DateOnly? RemovalDate { get; set; }
}
