using FluentValidation;
using SMS.Application.Features.StaffManagement.Commands;

namespace SMS.Application.Features.StaffManagement.Validators;

/// <summary>
/// Validator for CreateStaffCommand
/// </summary>
public class CreateStaffCommandValidator : AbstractValidator<CreateStaffCommand>
{
    public CreateStaffCommandValidator()
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

        RuleFor(x => x.Designation)
            .NotEmpty().WithMessage("Designation is required")
            .MaximumLength(100).WithMessage("Designation must not exceed 100 characters");

        // DepartmentId is optional

        RuleFor(x => x.RoleType)
            .IsInEnum().WithMessage("Valid Role Type is required");

        RuleFor(x => x.ExperienceYears)
            .GreaterThanOrEqualTo(0).WithMessage("Experience years must be 0 or greater")
            .LessThanOrEqualTo(100).WithMessage("Experience years must not exceed 100");

        RuleFor(x => x.JoiningDate)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Joining date must be in the past or today");

        // CreatedByUserId is set by controller
            
        RuleForEach(x => x.Qualifications).ChildRules(q => {
            q.RuleFor(x => x.Degree).NotEmpty().MaximumLength(100);
            q.RuleFor(x => x.Institution).NotEmpty().MaximumLength(200);
        });
    }
}

/// <summary>
/// Validator for UpdateStaffCommand
/// </summary>
public class UpdateStaffCommandValidator : AbstractValidator<UpdateStaffCommand>
{
    public UpdateStaffCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Staff ID is required")
            .Must(id => Guid.TryParse(id, out _)).WithMessage("Staff ID must be a valid GUID");

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

        RuleFor(x => x.Designation)
            .NotEmpty().WithMessage("Designation is required")
            .MaximumLength(100).WithMessage("Designation must not exceed 100 characters");

        // DepartmentId is optional

        RuleFor(x => x.ExperienceYears)
            .GreaterThanOrEqualTo(0).WithMessage("Experience years must be 0 or greater")
            .LessThanOrEqualTo(100).WithMessage("Experience years must not exceed 100");

        // UpdatedByUserId is set by controller
    }
}

/// <summary>
/// Validator for DeactivateStaffCommand
/// </summary>
public class DeactivateStaffCommandValidator : AbstractValidator<DeactivateStaffCommand>
{
    public DeactivateStaffCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Staff ID is required")
            .Must(id => Guid.TryParse(id, out _)).WithMessage("Staff ID must be a valid GUID");

        // UpdatedByUserId is set by controller
    }
}

/// <summary>
/// Validator for ActivateStaffCommand
/// </summary>
public class ActivateStaffCommandValidator : AbstractValidator<ActivateStaffCommand>
{
    public ActivateStaffCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Staff ID is required")
            .Must(id => Guid.TryParse(id, out _)).WithMessage("Staff ID must be a valid GUID");

        // UpdatedByUserId is set by controller
    }
}

/// <summary>
/// Validator for DeleteStaffCommand
/// </summary>
public class DeleteStaffCommandValidator : AbstractValidator<DeleteStaffCommand>
{
    public DeleteStaffCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Staff ID is required")
            .Must(id => Guid.TryParse(id, out _)).WithMessage("Staff ID must be a valid GUID");
    }
}
