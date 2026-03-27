using MediatR;
using SMS.Application.Features.Departments.DTOs;

namespace SMS.Application.Features.Departments.Queries;

public class GetAllDepartmentsQuery : IRequest<List<DepartmentListDto>>
{
    public string? SearchTerm { get; set; }
}

public class GetDepartmentByIdQuery : IRequest<DepartmentDto?>
{
    public Guid Id { get; set; }
}
