using FluentValidation;
using SMS.Application.Features.Teachers.Commands;

namespace SMS.Application.Features.Teachers.Validators;

/// <summary>
/// Validator for CreateTeacherCommand
/// </summary>
public class CreateTeacherCommandValidator : AbstractValidator<CreateTeacherCommand>
{
    public CreateTeacherCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(50).WithMessage("First name must not exceed 50 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(50).WithMessage("Last name must not exceed 50 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be a valid email address")
            .MaximumLength(100).WithMessage("Email must not exceed 100 characters");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required")
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Phone must be a valid phone number (E.164 format)")
            .MaximumLength(20).WithMessage("Phone must not exceed 20 characters");

        RuleFor(x => x.Qualification)
            .NotEmpty().WithMessage("Qualification is required")
            .MaximumLength(500).WithMessage("Qualification must not exceed 500 characters");

        RuleFor(x => x.ExperienceYears)
            .GreaterThanOrEqualTo(0).WithMessage("Experience years must be 0 or greater")
            .LessThanOrEqualTo(100).WithMessage("Experience years must not exceed 100");

        RuleFor(x => x.JoiningDate)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Joining date must be in the past or today");

        RuleFor(x => x.CreatedByUserId)
            .NotEmpty().WithMessage("Created by user ID is required")
            .Must(id => Guid.TryParse(id, out _)).WithMessage("Created by user ID must be a valid GUID");
    }
}

/// <summary>
/// Validator for UpdateTeacherCommand
/// </summary>
public class UpdateTeacherCommandValidator : AbstractValidator<UpdateTeacherCommand>
{
    public UpdateTeacherCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Teacher ID is required")
            .Must(id => Guid.TryParse(id, out _)).WithMessage("Teacher ID must be a valid GUID");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(50).WithMessage("First name must not exceed 50 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(50).WithMessage("Last name must not exceed 50 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be a valid email address")
            .MaximumLength(100).WithMessage("Email must not exceed 100 characters");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required")
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Phone must be a valid phone number (E.164 format)")
            .MaximumLength(20).WithMessage("Phone must not exceed 20 characters");

        RuleFor(x => x.Qualification)
            .NotEmpty().WithMessage("Qualification is required")
            .MaximumLength(500).WithMessage("Qualification must not exceed 500 characters");

        RuleFor(x => x.ExperienceYears)
            .GreaterThanOrEqualTo(0).WithMessage("Experience years must be 0 or greater")
            .LessThanOrEqualTo(100).WithMessage("Experience years must not exceed 100");

        RuleFor(x => x.UpdatedByUserId)
            .NotEmpty().WithMessage("Updated by user ID is required")
            .Must(id => Guid.TryParse(id, out _)).WithMessage("Updated by user ID must be a valid GUID");
    }
}

/// <summary>
/// Validator for DeactivateTeacherCommand
/// </summary>
public class DeactivateTeacherCommandValidator : AbstractValidator<DeactivateTeacherCommand>
{
    public DeactivateTeacherCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Teacher ID is required")
            .Must(id => Guid.TryParse(id, out _)).WithMessage("Teacher ID must be a valid GUID");

        RuleFor(x => x.UpdatedByUserId)
            .NotEmpty().WithMessage("Updated by user ID is required")
            .Must(id => Guid.TryParse(id, out _)).WithMessage("Updated by user ID must be a valid GUID");
    }
}

/// <summary>
/// Validator for ActivateTeacherCommand
/// </summary>
public class ActivateTeacherCommandValidator : AbstractValidator<ActivateTeacherCommand>
{
    public ActivateTeacherCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Teacher ID is required")
            .Must(id => Guid.TryParse(id, out _)).WithMessage("Teacher ID must be a valid GUID");

        RuleFor(x => x.UpdatedByUserId)
            .NotEmpty().WithMessage("Updated by user ID is required")
            .Must(id => Guid.TryParse(id, out _)).WithMessage("Updated by user ID must be a valid GUID");
    }
}

/// <summary>
/// Validator for DeleteTeacherCommand
/// </summary>
public class DeleteTeacherCommandValidator : AbstractValidator<DeleteTeacherCommand>
{
    public DeleteTeacherCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Teacher ID is required")
            .Must(id => Guid.TryParse(id, out _)).WithMessage("Teacher ID must be a valid GUID");
    }
}
