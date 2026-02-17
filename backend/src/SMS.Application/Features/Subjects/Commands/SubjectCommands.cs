using MediatR;
using SMS.Application.Features.Subjects.DTOs;

namespace SMS.Application.Features.Subjects.Commands;

/// <summary>
/// Command for creating a new subject
/// </summary>
public class CreateSubjectCommand : IRequest<SubjectDto>
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? Credits { get; set; }
    public int DisplayOrder { get; set; }
}

/// <summary>
/// Command for updating an existing subject
/// </summary>
public class UpdateSubjectCommand : IRequest<SubjectDto>
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? Credits { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
}

/// <summary>
/// Command for deleting a subject
/// </summary>
public class DeleteSubjectCommand : IRequest<bool>
{
    public string Id { get; set; } = string.Empty;
}
