using MediatR;
using SMS.Application.Students.DTOs;

namespace SMS.Application.Students.Queries;

public class GetStudentByIdQuery : IRequest<StudentDto>
{
    public Guid Id { get; set; }
}

public class GetAllStudentsQuery : IRequest<PagedResult<StudentDto>>
{
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
    public string? City { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
