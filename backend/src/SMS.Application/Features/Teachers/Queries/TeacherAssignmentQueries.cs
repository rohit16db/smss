using MediatR;
using SMS.Application.Features.Teachers.DTOs;

namespace SMS.Application.Features.Teachers.Queries;

/// <summary>
/// Query to get all assignments for a teacher
/// </summary>
public class GetTeacherAssignmentsQuery : IRequest<List<TeacherAssignmentDto>>
{
    public Guid TeacherId { get; set; }
    public bool? ActiveOnly { get; set; }
}
