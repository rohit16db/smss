using FluentValidation;
using SMS.Application.Features.Attendance.Commands;
using SMS.Domain.Enums;

namespace SMS.Application.Features.Attendance.Validators;

/// <summary>
/// Validator for MarkStudentAttendanceCommand
/// </summary>
public class MarkStudentAttendanceCommandValidator : AbstractValidator<MarkStudentAttendanceCommand>
{
    public MarkStudentAttendanceCommandValidator()
    {
        RuleFor(x => x.StudentId)
            .NotEmpty()
            .Must(BeValidGuid)
            .WithMessage("Student ID must be a valid GUID");

        RuleFor(x => x.ClassId)
            .NotEmpty()
            .Must(BeValidGuid)
            .WithMessage("Class ID must be a valid GUID");

        RuleFor(x => x.AttendanceDate)
            .NotEmpty()
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
            .WithMessage("Attendance date cannot be more than 1 day in the future");

        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(AttendanceStatus.IsValid)
            .WithMessage($"Status must be one of: {string.Join(", ", AttendanceStatus.ValidStatuses)}");

        RuleFor(x => x.Reason)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Reason));

        RuleFor(x => x.CreatedByUserId)
            .NotEmpty()
            .Must(BeValidGuid)
            .WithMessage("Created by user ID must be a valid GUID");
    }

    private bool BeValidGuid(string id) => Guid.TryParse(id, out _);
}

/// <summary>
/// Validator for UpdateStudentAttendanceCommand
/// </summary>
public class UpdateStudentAttendanceCommandValidator : AbstractValidator<UpdateStudentAttendanceCommand>
{
    public UpdateStudentAttendanceCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .Must(BeValidGuid)
            .WithMessage("ID must be a valid GUID");

        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(AttendanceStatus.IsValid)
            .WithMessage($"Status must be one of: {string.Join(", ", AttendanceStatus.ValidStatuses)}");

        RuleFor(x => x.Reason)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Reason));

        RuleFor(x => x.UpdatedByUserId)
            .NotEmpty()
            .Must(BeValidGuid)
            .WithMessage("Updated by user ID must be a valid GUID");
    }

    private bool BeValidGuid(string id) => Guid.TryParse(id, out _);
}

/// <summary>
/// Validator for DeleteStudentAttendanceCommand
/// </summary>
public class DeleteStudentAttendanceCommandValidator : AbstractValidator<DeleteStudentAttendanceCommand>
{
    public DeleteStudentAttendanceCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .Must(BeValidGuid)
            .WithMessage("ID must be a valid GUID");
    }

    private bool BeValidGuid(string id) => Guid.TryParse(id, out _);
}

/// <summary>
/// Validator for RecordTeacherAttendanceCommand
/// </summary>
public class RecordTeacherAttendanceCommandValidator : AbstractValidator<RecordTeacherAttendanceCommand>
{
    public RecordTeacherAttendanceCommandValidator()
    {
        RuleFor(x => x.TeacherId)
            .NotEmpty()
            .Must(BeValidGuid)
            .WithMessage("Teacher ID must be a valid GUID");

        RuleFor(x => x.AttendanceDate)
            .NotEmpty()
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
            .WithMessage("Attendance date cannot be more than 1 day in the future");

        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(AttendanceStatus.IsValid)
            .WithMessage($"Status must be one of: {string.Join(", ", AttendanceStatus.ValidStatuses)}");

        RuleFor(x => x.Reason)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Reason));

        RuleFor(x => x.CreatedByUserId)
            .NotEmpty()
            .Must(BeValidGuid)
            .WithMessage("Created by user ID must be a valid GUID");
    }

    private bool BeValidGuid(string id) => Guid.TryParse(id, out _);
}

/// <summary>
/// Validator for UpdateTeacherAttendanceCommand
/// </summary>
public class UpdateTeacherAttendanceCommandValidator : AbstractValidator<UpdateTeacherAttendanceCommand>
{
    public UpdateTeacherAttendanceCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .Must(BeValidGuid)
            .WithMessage("ID must be a valid GUID");

        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(AttendanceStatus.IsValid)
            .WithMessage($"Status must be one of: {string.Join(", ", AttendanceStatus.ValidStatuses)}");

        RuleFor(x => x.Reason)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Reason));

        RuleFor(x => x.UpdatedByUserId)
            .NotEmpty()
            .Must(BeValidGuid)
            .WithMessage("Updated by user ID must be a valid GUID");
    }

    private bool BeValidGuid(string id) => Guid.TryParse(id, out _);
}

/// <summary>
/// Validator for DeleteTeacherAttendanceCommand
/// </summary>
public class DeleteTeacherAttendanceCommandValidator : AbstractValidator<DeleteTeacherAttendanceCommand>
{
    public DeleteTeacherAttendanceCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .Must(BeValidGuid)
            .WithMessage("ID must be a valid GUID");
    }

    private bool BeValidGuid(string id) => Guid.TryParse(id, out _);
}
