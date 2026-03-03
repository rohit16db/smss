using FluentValidation;
using SMS.Application.Features.Exams.Commands;

namespace SMS.Application.Features.Exams.Validators;

/// <summary>
/// Validator for SaveStudentMarksCommand
/// Single Responsibility: Validate marks save request data
/// </summary>
public class SaveStudentMarksCommandValidator : AbstractValidator<SaveStudentMarksCommand>
{
    public SaveStudentMarksCommandValidator()
    {
        RuleFor(x => x.ExamId)
            .NotEmpty().WithMessage("Exam ID is required");

        RuleFor(x => x.ClassId)
            .NotEmpty().WithMessage("Class ID is required");

        RuleFor(x => x.MarksData)
            .NotEmpty().WithMessage("At least one student's marks must be provided");
    }
}

/// <summary>
/// Validator for SubmitMarksCommand
/// Single Responsibility: Validate marks submission request
/// </summary>
public class SubmitMarksCommandValidator : AbstractValidator<SubmitMarksCommand>
{
    public SubmitMarksCommandValidator()
    {
        RuleFor(x => x.ExamId)
            .NotEmpty().WithMessage("Exam ID is required");

        RuleFor(x => x.ClassId)
            .NotEmpty().WithMessage("Class ID is required");

        RuleFor(x => x.ConfirmedById)
            .NotEmpty().WithMessage("Confirmed by user ID is required");
    }
}

/// <summary>
/// Validator for GenerateReportCardCommand
/// Single Responsibility: Validate report card generation request
/// </summary>
public class GenerateReportCardCommandValidator : AbstractValidator<GenerateReportCardCommand>
{
    public GenerateReportCardCommandValidator()
    {
        RuleFor(x => x.ExamId)
            .NotEmpty().WithMessage("Exam ID is required");

        RuleFor(x => x.StudentId)
            .NotEmpty().WithMessage("Student ID is required");
    }
}
