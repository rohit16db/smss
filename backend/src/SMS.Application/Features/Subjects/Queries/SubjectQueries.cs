using MediatR;
using SMS.Application.Features.Subjects.DTOs;

namespace SMS.Application.Features.Subjects.Queries;

/// <summary>
/// Query for getting all subjects with pagination
/// </summary>
public class GetAllSubjectsQuery : IRequest<PaginatedSubjectListDto>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
}

/// <summary>
/// Query for getting a subject by ID
/// </summary>
public class GetSubjectByIdQuery : IRequest<SubjectDto?>
{
    public string Id { get; set; } = string.Empty;
}

/// <summary>
/// Query for getting all active subjects (for dropdowns)
/// </summary>
public class GetActiveSubjectsQuery : IRequest<List<SubjectListDto>>
{
}
