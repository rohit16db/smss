using FluentValidation;
using SMS.Application.Features.Holidays.Commands;

namespace SMS.Application.Features.Holidays.Validators;

/// <summary>
/// Validator for CreateHolidayCommand
/// </summary>
public class CreateHolidayCommandValidator : AbstractValidator<CreateHolidayCommand>
{
    public CreateHolidayCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Holiday name is required")
            .MaximumLength(200).WithMessage("Holiday name cannot exceed 200 characters");

        RuleFor(x => x.HolidayDate)
            .NotEmpty().WithMessage("Holiday date is required");

        RuleFor(x => x.AcademicYearId)
            // .NotEmpty().WithMessage("Academic year ID is required") // Optional now since it can be taken from context
            .Must(id => string.IsNullOrEmpty(id) || Guid.TryParse(id, out _)).WithMessage("Academic year ID must be a valid GUID");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Type)
            .MaximumLength(50).WithMessage("Type cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.Type));
    }
}

/// <summary>
/// Validator for UpdateHolidayCommand
/// </summary>
public class UpdateHolidayCommandValidator : AbstractValidator<UpdateHolidayCommand>
{
    public UpdateHolidayCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Holiday ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Holiday name is required")
            .MaximumLength(200).WithMessage("Holiday name cannot exceed 200 characters");

        RuleFor(x => x.HolidayDate)
            .NotEmpty().WithMessage("Holiday date is required");

        RuleFor(x => x.AcademicYearId)
            .Must(id => string.IsNullOrEmpty(id) || Guid.TryParse(id, out _)).WithMessage("Academic year ID must be a valid GUID");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Type)
            .MaximumLength(50).WithMessage("Type cannot exceed 50 characters")
            .When(x => !string.IsNullOrEmpty(x.Type));
    }
}
