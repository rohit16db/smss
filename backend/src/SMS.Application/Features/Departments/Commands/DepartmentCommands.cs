using MediatR;
using SMS.Application.Features.Departments.DTOs;

namespace SMS.Application.Features.Departments.Commands;

public class CreateDepartmentCommand : IRequest<DepartmentDto>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? HeadOfDepartmentId { get; set; }
}

public class UpdateDepartmentCommand : IRequest<DepartmentDto>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? HeadOfDepartmentId { get; set; }
}

public class DeleteDepartmentCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}
