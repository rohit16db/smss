using FluentValidation;
using SMS.Application.Features.Exams.Commands;

namespace SMS.Application.Features.Exams.Validators;

/// <summary>
/// Validator for CreateExamCommand
/// Single Responsibility: Validate exam creation request data
/// </summary>
public class CreateExamCommandValidator : AbstractValidator<CreateExamCommand>
{
    public CreateExamCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Exam name is required")
            .MaximumLength(255).WithMessage("Exam name cannot exceed 255 characters");

        RuleFor(x => x.ExamDate)
            .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Exam date cannot be in the past");

        RuleFor(x => x.TotalMarks)
            .GreaterThan(0).WithMessage("Total marks must be greater than 0")
            .LessThanOrEqualTo(10000).WithMessage("Total marks cannot exceed 10000");

        RuleFor(x => x.PassMarks)
            .GreaterThan(0).WithMessage("Pass marks must be greater than 0")
            .LessThan(x => x.TotalMarks).WithMessage("Pass marks must be less than total marks");

        // Optional: Subjects and classes can be added after exam creation
        // RuleFor(x => x.SubjectIds)
        //     .NotEmpty().WithMessage("At least one subject must be assigned");

        // RuleFor(x => x.ClassIds)
        //     .NotEmpty().WithMessage("At least one class must be assigned");
    }
}

/// <summary>
/// Validator for UpdateExamCommand
/// Single Responsibility: Validate exam update request data
/// </summary>
public class UpdateExamCommandValidator : AbstractValidator<UpdateExamCommand>
{
    public UpdateExamCommandValidator()
    {
        RuleFor(x => x.ExamId)
            .NotEmpty().WithMessage("Exam ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Exam name is required")
            .MaximumLength(255).WithMessage("Exam name cannot exceed 255 characters");

        RuleFor(x => x.ExamDate)
            .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Exam date cannot be in the past");

        RuleFor(x => x.TotalMarks)
            .GreaterThan(0).WithMessage("Total marks must be greater than 0");

        RuleFor(x => x.PassMarks)
            .GreaterThan(0).WithMessage("Pass marks must be greater than 0")
            .LessThan(x => x.TotalMarks).WithMessage("Pass marks must be less than total marks");
    }
}

/// <summary>
/// Validator for PublishExamCommand
/// Single Responsibility: Validate exam publish request
/// </summary>
public class PublishExamCommandValidator : AbstractValidator<PublishExamCommand>
{
    public PublishExamCommandValidator()
    {
        RuleFor(x => x.ExamId)
            .NotEmpty().WithMessage("Exam ID is required");
    }
}

/// <summary>
/// Validator for DeleteExamCommand
/// Single Responsibility: Validate exam deletion request
/// </summary>
public class DeleteExamCommandValidator : AbstractValidator<DeleteExamCommand>
{
    public DeleteExamCommandValidator()
    {
        RuleFor(x => x.ExamId)
            .NotEmpty().WithMessage("Exam ID is required");
    }
}
