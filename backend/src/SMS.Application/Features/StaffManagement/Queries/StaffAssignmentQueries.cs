using MediatR;
using SMS.Application.Features.StaffManagement.DTOs;

namespace SMS.Application.Features.StaffManagement.Queries;

/// <summary>
/// Query to get all assignments for a staff member
/// </summary>
public class GetStaffAssignmentsQuery : IRequest<List<StaffAssignmentDto>>
{
    public Guid StaffId { get; set; }
    public bool? ActiveOnly { get; set; }
}

/// <summary>
/// Query to get all assignments for a section
/// </summary>
public class GetStaffAssignmentsBySectionQuery : IRequest<List<StaffAssignmentDto>>
{
    public Guid SectionId { get; set; }
    public Guid AcademicYearId { get; set; }
}
