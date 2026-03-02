using FluentValidation;
using SMS.Application.Features.Exams.Commands;

namespace SMS.Application.Features.Exams.Validators;

/// <summary>
/// Validator for ConfigureGradesCommand
/// Single Responsibility: Validate grade configuration request data
/// </summary>
public class ConfigureGradesCommandValidator : AbstractValidator<ConfigureGradesCommand>
{
    public ConfigureGradesCommandValidator()
    {
        RuleFor(x => x.Grades)
            .NotEmpty().WithMessage("At least one grade configuration must be provided");

        RuleForEach(x => x.Grades).ChildRules(gradeRules =>
        {
            gradeRules.RuleFor(g => g.Name)
                .NotEmpty().WithMessage("Grade name is required")
                .MaximumLength(10).WithMessage("Grade name cannot exceed 10 characters");

            gradeRules.RuleFor(g => g.MinPercentage)
                .GreaterThanOrEqualTo(0).WithMessage("Min percentage cannot be negative")
                .LessThanOrEqualTo(100).WithMessage("Min percentage cannot exceed 100");

            gradeRules.RuleFor(g => g.MaxPercentage)
                .GreaterThanOrEqualTo(0).WithMessage("Max percentage cannot be negative")
                .LessThanOrEqualTo(100).WithMessage("Max percentage cannot exceed 100")
                .GreaterThanOrEqualTo(g => g.MinPercentage)
                .WithMessage("Max percentage must be greater than or equal to min percentage");
        });
    }
}
