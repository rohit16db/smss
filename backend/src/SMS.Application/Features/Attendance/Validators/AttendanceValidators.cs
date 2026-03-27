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

        // SectionId validation removed - section is auto-detected from enrollment

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
/// Validator for RecordStaffAttendanceCommand
/// </summary>
public class RecordStaffAttendanceCommandValidator : AbstractValidator<RecordStaffAttendanceCommand>
{
    public RecordStaffAttendanceCommandValidator()
    {
        RuleFor(x => x.StaffId)
            .NotEmpty()
            .Must(BeValidGuid)
            .WithMessage("Staff ID must be a valid GUID");

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
/// Validator for UpdateStaffAttendanceCommand
/// </summary>
public class UpdateStaffAttendanceCommandValidator : AbstractValidator<UpdateStaffAttendanceCommand>
{
    public UpdateStaffAttendanceCommandValidator()
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
/// Validator for DeleteStaffAttendanceCommand
/// </summary>
public class DeleteStaffAttendanceCommandValidator : AbstractValidator<DeleteStaffAttendanceCommand>
{
    public DeleteStaffAttendanceCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .Must(BeValidGuid)
            .WithMessage("ID must be a valid GUID");
    }

    private bool BeValidGuid(string id) => Guid.TryParse(id, out _);
}
