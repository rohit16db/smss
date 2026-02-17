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
/// Get salary structures applicable for a teacher
/// </summary>
public class GetApplicableSalaryStructuresQuery : IRequest<List<SalaryStructureDto>>
{
    public Guid TeacherId { get; set; }
}

/// <summary>
/// Get current salary structure for a teacher
/// </summary>
public class GetTeacherCurrentSalaryStructureQuery : IRequest<TeacherSalaryAssignmentDto>
{
    public Guid TeacherId { get; set; }
}

/// <summary>
/// Get all teachers with their assigned salary structures
/// </summary>
public class GetTeachersWithSalaryStructuresQuery : IRequest<List<TeacherSalaryAssignmentDto>>
{
    public bool? IsActive { get; set; }
}
