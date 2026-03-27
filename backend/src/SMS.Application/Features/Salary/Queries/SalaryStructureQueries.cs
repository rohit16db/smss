using MediatR;
using SMS.Application.Features.Salary.DTOs;

namespace SMS.Application.Features.Salary.Queries;

/// <summary>
/// Get all salary structures
/// </summary>
public class GetAllSalaryStructuresQuery : IRequest<List<SalaryStructureDto>>
{
    public bool? IsActive { get; set; }
}

/// <summary>
/// Get salary structure by ID
/// </summary>
public class GetSalaryStructureByIdQuery : IRequest<SalaryStructureDto>
{
    public Guid Id { get; set; }
}

/// <summary>
/// Get salary structures applicable for a staff member
/// </summary>
public class GetApplicableSalaryStructuresQuery : IRequest<List<SalaryStructureDto>>
{
    public Guid StaffId { get; set; }
}

/// <summary>
/// Get current salary structure for a staff member
/// </summary>
public class GetStaffCurrentSalaryStructureQuery : IRequest<StaffSalaryAssignmentDto>
{
    public Guid StaffId { get; set; }
}

/// <summary>
/// Get all staff with their assigned salary structures
/// </summary>
public class GetStaffWithSalaryStructuresQuery : IRequest<List<StaffSalaryAssignmentDto>>
{
    public bool? IsActive { get; set; }
}
