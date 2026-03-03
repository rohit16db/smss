using MediatR;
using SMS.Application.Features.Exams.DTOs;

namespace SMS.Application.Features.Exams.Commands;

/// <summary>
/// Command to save student marks (Draft mode)
/// Single Responsibility: Request marks save without finalization
/// </summary>
public class SaveStudentMarksCommand : IRequest<SaveMarksResponseDto>
{
    public Guid ExamId { get; set; }
    public Guid ClassId { get; set; }
    public Guid SectionId { get; set; }
    public List<StudentMarksEntryDto> MarksData { get; set; } = new();
}

/// <summary>
/// Command to submit marks for a class
/// Single Responsibility: Request marks submission and report card generation
/// </summary>
public class SubmitMarksCommand : IRequest<SaveMarksResponseDto>
{
    public Guid ExamId { get; set; }
    public Guid ClassId { get; set; }
    public Guid SectionId { get; set; }
    public Guid ConfirmedById { get; set; }
}

/// <summary>
/// Command to generate report card for a student
/// Single Responsibility: Request report card generation after marks submission
/// </summary>
public class GenerateReportCardCommand : IRequest<Unit>
{
    public Guid ExamId { get; set; }
    public Guid StudentId { get; set; }
    public Guid GeneratedBy { get; set; }
}
