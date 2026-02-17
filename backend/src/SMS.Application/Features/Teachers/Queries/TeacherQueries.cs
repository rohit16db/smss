using MediatR;
using SMS.Application.Features.Teachers.DTOs;

namespace SMS.Application.Features.Teachers.Queries;

/// <summary>
/// Query to get a teacher by ID
/// </summary>
public class GetTeacherByIdQuery : IRequest<TeacherDto?>
{
    public required string Id { get; set; }
}

/// <summary>
/// Query to get all teachers with pagination
/// </summary>
public class GetAllTeachersQuery : IRequest<PaginatedTeacherListDto>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
}

/// <summary>
/// Query to get a teacher by email
/// </summary>
public class GetTeacherByEmailQuery : IRequest<TeacherDto?>
{
    public required string Email { get; set; }
}

/// <summary>
/// Query to check if a teacher with email exists
/// </summary>
public class TeacherEmailExistsQuery : IRequest<bool>
{
    public required string Email { get; set; }
    public string? ExcludeTeacherId { get; set; }
}

/// <summary>
/// Query to get active teachers only
/// </summary>
public class GetActiveTeachersQuery : IRequest<List<TeacherListDto>>
{
    public string? SearchTerm { get; set; }
}
