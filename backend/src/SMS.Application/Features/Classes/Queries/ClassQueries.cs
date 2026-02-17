using MediatR;
using SMS.Application.Features.Classes.DTOs;

namespace SMS.Application.Features.Classes.Queries;

/// <summary>
/// Get all classes with pagination
/// </summary>
public class GetAllClassesQuery : IRequest<PaginatedClassListDto>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
}

/// <summary>
/// Get a specific class by ID with all its sections
/// </summary>
public class GetClassByIdQuery : IRequest<ClassDto?>
{
    public string Id { get; set; } = string.Empty;
}

/// <summary>
/// Get all sections for a specific class
/// </summary>
public class GetSectionsByClassIdQuery : IRequest<List<SectionListDto>>
{
    public string ClassId { get; set; } = string.Empty;
}

/// <summary>
/// Get section details by ID
/// </summary>
public class GetSectionByIdQuery : IRequest<SectionDto?>
{
    public string Id { get; set; } = string.Empty;
}

/// <summary>
/// Get student section history (all past and current sections)
/// </summary>
public class GetStudentSectionHistoryQuery : IRequest<StudentSectionHistoryDto>
{
    public string StudentId { get; set; } = string.Empty;
}

/// <summary>
/// Get current section for a student
/// </summary>
public class GetStudentCurrentSectionQuery : IRequest<StudentSectionDto?>
{
    public string StudentId { get; set; } = string.Empty;
}
