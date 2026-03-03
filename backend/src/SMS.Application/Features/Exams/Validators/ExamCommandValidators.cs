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

        RuleFor(x => x.StartDate)
            .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Exam start date cannot be in the past");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("Exam end date must be on or after start date");

        RuleFor(x => x.TotalMarks)
            .GreaterThan(0).WithMessage("Total marks must be greater than 0")
            .LessThanOrEqualTo(10000).WithMessage("Total marks cannot exceed 10000");

        RuleFor(x => x.PassMarks)
            .GreaterThan(0).WithMessage("Pass marks must be greater than 0")
            .LessThan(x => x.TotalMarks).WithMessage("Pass marks must be less than total marks");

        RuleForEach(x => x.Subjects)
            .SetValidator(new ExamSubjectInputValidator());
    }
}

public class ExamSubjectInputValidator : AbstractValidator<ExamSubjectInput>
{
    public ExamSubjectInputValidator()
    {
        RuleFor(x => x.SubjectId)
            .NotEmpty().WithMessage("Subject ID is required");

        RuleFor(x => x.MaxMarks)
            .GreaterThan(0).WithMessage("Max marks must be greater than 0")
            .LessThanOrEqualTo(1000).WithMessage("Max marks cannot exceed 1000");

        RuleFor(x => x.PassMarks)
            .GreaterThan(0).WithMessage("Pass marks must be greater than 0")
            .LessThan(x => x.MaxMarks).WithMessage("Pass marks must be less than max marks");
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

        RuleFor(x => x.StartDate)
            .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Start date cannot be in the past");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("End date must be greater than or equal to start date");

        RuleFor(x => x.TotalMarks)
            .GreaterThan(0).WithMessage("Total marks must be greater than 0");

        RuleFor(x => x.PassMarks)
            .GreaterThan(0).WithMessage("Pass marks must be greater than 0")
            .LessThan(x => x.TotalMarks).WithMessage("Pass marks must be less than total marks");

        RuleForEach(x => x.Subjects)
            .SetValidator(new ExamSubjectInputValidator());

        RuleFor(x => x.ClassIds)
            .NotEmpty().WithMessage("At least one class must be selected");
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
