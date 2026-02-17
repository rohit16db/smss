using MediatR;
using SMS.Application.Features.Classes.DTOs;

namespace SMS.Application.Features.Classes.Commands;

/// <summary>
/// Create a new class
/// </summary>
public class CreateClassCommand : IRequest<ClassDto>
{
    public string Name { get; set; } = string.Empty;
    public string? AcademicYear { get; set; }
}

/// <summary>
/// Update an existing class
/// </summary>
public class UpdateClassCommand : IRequest<ClassDto>
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? AcademicYear { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Delete a class
/// </summary>
public class DeleteClassCommand : IRequest<bool>
{
    public string Id { get; set; } = string.Empty;
}

/// <summary>
/// Create a new section within a class
/// </summary>
public class CreateSectionCommand : IRequest<SectionDto>
{
    public string ClassId { get; set; } = string.Empty;
    public string SectionName { get; set; } = string.Empty;
}

/// <summary>
/// Update an existing section
/// </summary>
public class UpdateSectionCommand : IRequest<SectionDto>
{
    public string Id { get; set; } = string.Empty;
    public string SectionName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

/// <summary>
/// Delete a section
/// </summary>
public class DeleteSectionCommand : IRequest<bool>
{
    public string Id { get; set; } = string.Empty;
}

/// <summary>
/// Move a student to a different section
/// </summary>
public class MoveStudentSectionCommand : IRequest<StudentSectionDto>
{
    public string StudentId { get; set; } = string.Empty;
    public string NewSectionId { get; set; } = string.Empty;
}
