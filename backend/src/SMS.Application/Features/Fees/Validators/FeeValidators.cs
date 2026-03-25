using FluentValidation;
using SMS.Application.Features.Fees.Commands;
using SMS.Domain.Enums;

namespace SMS.Application.Features.Fees.Validators;

/// <summary>
/// Validator for CreateFeeStructureCommand
/// </summary>
public class CreateFeeStructureCommandValidator : AbstractValidator<CreateFeeStructureCommand>
{
    public CreateFeeStructureCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Fee structure name is required")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters");

        RuleFor(x => x.AcademicYearId)
            .NotEmpty().WithMessage("Academic year ID is required")
            .Must(id => string.IsNullOrEmpty(id) || Guid.TryParse(id, out _)).WithMessage("Academic year ID must be a valid GUID");

        RuleFor(x => x.Frequency)
            .NotEmpty().WithMessage("Frequency is required")
            .Must(FeeFrequency.IsValid).WithMessage($"Frequency must be one of: {string.Join(", ", FeeFrequency.ValidFrequencies)}");

        RuleFor(x => x.Categories)
            .NotEmpty().WithMessage("At least one category is required")
            .Must(categories => categories.Count > 0).WithMessage("Fee structure must have at least one category");

        RuleForEach(x => x.Categories).ChildRules(category =>
        {
            category.RuleFor(c => c.Category)
                .NotEmpty().WithMessage("Category name is required")
                .MaximumLength(100).WithMessage("Category name must not exceed 100 characters");

            category.RuleFor(c => c.Amount)
                .GreaterThan(0).WithMessage("Category amount must be greater than 0");
        });

        RuleFor(x => x.CreatedByUserId)
            .NotEmpty().WithMessage("Created by user ID is required")
            .Must(id => Guid.TryParse(id, out _)).WithMessage("Created by user ID must be a valid GUID");
    }
}

/// <summary>
/// Validator for UpdateFeeStructureCommand
/// </summary>
public class UpdateFeeStructureCommandValidator : AbstractValidator<UpdateFeeStructureCommand>
{
    public UpdateFeeStructureCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Fee structure ID is required")
            .Must(id => Guid.TryParse(id, out _)).WithMessage("Fee structure ID must be a valid GUID");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Fee structure name is required")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters");

        RuleFor(x => x.AcademicYearId)
            .NotEmpty().WithMessage("Academic year ID is required")
            .Must(id => string.IsNullOrEmpty(id) || Guid.TryParse(id, out _)).WithMessage("Academic year ID must be a valid GUID");

        RuleFor(x => x.Frequency)
            .NotEmpty().WithMessage("Frequency is required")
            .Must(FeeFrequency.IsValid).WithMessage($"Frequency must be one of: {string.Join(", ", FeeFrequency.ValidFrequencies)}");

        RuleFor(x => x.Categories)
            .NotEmpty().WithMessage("At least one category is required")
            .Must(categories => categories.Count > 0).WithMessage("Fee structure must have at least one category");

        RuleForEach(x => x.Categories).ChildRules(category =>
        {
            category.RuleFor(c => c.Category)
                .NotEmpty().WithMessage("Category name is required")
                .MaximumLength(100).WithMessage("Category name must not exceed 100 characters");

            category.RuleFor(c => c.Amount)
                .GreaterThan(0).WithMessage("Category amount must be greater than 0");
        });

        RuleFor(x => x.UpdatedByUserId)
            .NotEmpty().WithMessage("Updated by user ID is required")
            .Must(id => Guid.TryParse(id, out _)).WithMessage("Updated by user ID must be a valid GUID");
    }
}

/// <summary>
/// Validator for DeleteFeeStructureCommand
/// </summary>
public class DeleteFeeStructureCommandValidator : AbstractValidator<DeleteFeeStructureCommand>
{
    public DeleteFeeStructureCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Fee structure ID is required")
            .Must(id => Guid.TryParse(id, out _)).WithMessage("Fee structure ID must be a valid GUID");
    }
}

/// <summary>
/// Validator for AssignStudentFeeCommand
/// </summary>
public class AssignStudentFeeCommandValidator : AbstractValidator<AssignStudentFeeCommand>
{
    public AssignStudentFeeCommandValidator()
    {
        RuleFor(x => x.StudentId)
            .NotEmpty().WithMessage("Student ID is required");
            // Accept both GUID and enrollment number format

        RuleFor(x => x.FeeStructureId)
            .NotEmpty().WithMessage("Fee structure ID is required")
            .Must(id => Guid.TryParse(id, out _)).WithMessage("Fee structure ID must be a valid GUID");

        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(DateTime.UtcNow.AddYears(1)).WithMessage("Start date must be within next year");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date")
            .When(x => x.EndDate.HasValue);

        RuleFor(x => x.CreatedByUserId)
            .NotEmpty().WithMessage("Created by user ID is required")
            .Must(id => Guid.TryParse(id, out _)).WithMessage("Created by user ID must be a valid GUID");
    }
}

/// <summary>
/// Validator for RecordFeePaymentCommand
/// </summary>
public class RecordFeePaymentCommandValidator : AbstractValidator<RecordFeePaymentCommand>
{
    public RecordFeePaymentCommandValidator()
    {
        RuleFor(x => x.StudentFeeId)
            .NotEmpty().WithMessage("Student fee ID is required")
            .Must(id => Guid.TryParse(id, out _)).WithMessage("Student fee ID must be a valid GUID");

        RuleFor(x => x.AmountPaid)
            .GreaterThan(0).WithMessage("Amount paid must be greater than 0");

        RuleFor(x => x.PaymentDate)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Payment date cannot be in the future");

        RuleFor(x => x.PaymentMethod)
            .NotEmpty().WithMessage("Payment method is required")
            .Must(PaymentMethod.IsValid).WithMessage($"Payment method must be one of: {string.Join(", ", PaymentMethod.ValidMethods)}");

        RuleFor(x => x.CreatedByUserId)
            .NotEmpty().WithMessage("Created by user ID is required")
            .Must(id => Guid.TryParse(id, out _)).WithMessage("Created by user ID must be a valid GUID");
    }
}

/// <summary>
/// Validator for BulkAssignStudentFeeCommand
/// </summary>
public class BulkAssignStudentFeeCommandValidator : AbstractValidator<BulkAssignStudentFeeCommand>
{
    public BulkAssignStudentFeeCommandValidator()
    {
        RuleFor(x => x.FeeStructureId)
            .NotEmpty().WithMessage("Fee structure ID is required")
            .Must(id => Guid.TryParse(id, out _)).WithMessage("Fee structure ID must be a valid GUID");

        RuleFor(x => x.SectionId)
            .NotEmpty().WithMessage("Section ID is required")
            .Must(id => Guid.TryParse(id, out _)).WithMessage("Section ID must be a valid GUID");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required")
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(365)).WithMessage("Start date cannot be more than 1 year in the future");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date")
            .When(x => x.EndDate.HasValue);

        RuleFor(x => x.CreatedByUserId)
            .NotEmpty().WithMessage("Created by user ID is required")
            .Must(id => Guid.TryParse(id, out _)).WithMessage("Created by user ID must be a valid GUID");
    }
}
