using FluentValidation;
using SMS.Application.Students.Commands;

namespace SMS.Application.Students.Validators;

public class CreateStudentCommandValidator : AbstractValidator<CreateStudentCommand>
{
    public CreateStudentCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(50).WithMessage("First name must not exceed 50 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(50).WithMessage("Last name must not exceed 50 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(100).WithMessage("Email must not exceed 100 characters");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("Date of birth is required")
            .LessThan(DateTime.UtcNow).WithMessage("Date of birth must be in the past")
            .GreaterThan(DateTime.UtcNow.AddYears(-100)).WithMessage("Invalid date of birth");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

        RuleFor(x => x.City)
            .MaximumLength(50).WithMessage("City must not exceed 50 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.City));

        RuleFor(x => x.State)
            .MaximumLength(50).WithMessage("State must not exceed 50 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.State));

        RuleFor(x => x.PostalCode)
            .MaximumLength(10).WithMessage("Postal code must not exceed 10 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.PostalCode));
    }
}

public class UpdateStudentCommandValidator : AbstractValidator<UpdateStudentCommand>
{
    public UpdateStudentCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Student ID is required");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(50).WithMessage("First name must not exceed 50 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(50).WithMessage("Last name must not exceed 50 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(100).WithMessage("Email must not exceed 100 characters");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("Date of birth is required")
            .LessThan(DateTime.UtcNow).WithMessage("Date of birth must be in the past")
            .GreaterThan(DateTime.UtcNow.AddYears(-100)).WithMessage("Invalid date of birth");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

        RuleFor(x => x.City)
            .MaximumLength(50).WithMessage("City must not exceed 50 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.City));

        RuleFor(x => x.State)
            .MaximumLength(50).WithMessage("State must not exceed 50 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.State));

        RuleFor(x => x.PostalCode)
            .MaximumLength(10).WithMessage("Postal code must not exceed 10 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.PostalCode));
    }
}
