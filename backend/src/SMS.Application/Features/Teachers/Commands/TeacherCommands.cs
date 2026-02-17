using MediatR;
using SMS.Application.Features.Teachers.DTOs;

namespace SMS.Application.Features.Teachers.Commands;

/// <summary>
/// Command to create a new teacher
/// </summary>
public class CreateTeacherCommand : IRequest<TeacherDto>
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string Phone { get; set; }
    public required string Qualification { get; set; }
    public required int ExperienceYears { get; set; }
    public required DateTime JoiningDate { get; set; }
    public required string CreatedByUserId { get; set; }
}

/// <summary>
/// Command to update teacher information
/// </summary>
public class UpdateTeacherCommand : IRequest<TeacherDto>
{
    public required string Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string Phone { get; set; }
    public required string Qualification { get; set; }
    public required int ExperienceYears { get; set; }
    public required bool IsActive { get; set; }
    public required string UpdatedByUserId { get; set; }
}

/// <summary>
/// Command to deactivate a teacher
/// </summary>
public class DeactivateTeacherCommand : IRequest<bool>
{
    public required string Id { get; set; }
    public required string UpdatedByUserId { get; set; }
}

/// <summary>
/// Command to activate a teacher
/// </summary>
public class ActivateTeacherCommand : IRequest<bool>
{
    public required string Id { get; set; }
    public required string UpdatedByUserId { get; set; }
}

/// <summary>
/// Command to delete a teacher
/// </summary>
public class DeleteTeacherCommand : IRequest<bool>
{
    public required string Id { get; set; }
}
