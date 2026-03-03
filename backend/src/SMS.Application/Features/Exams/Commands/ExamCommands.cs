using MediatR;
using SMS.Application.Features.Exams.DTOs;

namespace SMS.Application.Features.Exams.Commands;

/// <summary>
/// Command to create a new exam
/// Single Responsibility: Request exam creation with validated data
/// </summary>
public class CreateExamCommand : IRequest<ExamDto>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalMarks { get; set; } = 100;
    public decimal PassMarks { get; set; } = 40;
    public List<ExamSubjectInput> Subjects { get; set; } = new();
    public List<Guid> ClassIds { get; set; } = new();
    public Guid CreatedById { get; set; }
}

/// <summary>
/// Input for exam subject with max marks
/// </summary>
public class ExamSubjectInput
{
    public Guid SubjectId { get; set; }
    public decimal MaxMarks { get; set; }
    public decimal PassMarks { get; set; } = 40;
}

/// <summary>
/// Command to update an existing exam
/// Single Responsibility: Request exam update (only when Draft)
/// </summary>
public class UpdateExamCommand : IRequest<ExamDto>
{
    public Guid ExamId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalMarks { get; set; }
    public decimal PassMarks { get; set; }
    public List<ExamSubjectInput> Subjects { get; set; } = new();
    public List<Guid> ClassIds { get; set; } = new();
}

/// <summary>
/// Command to publish an exam
/// Single Responsibility: Request exam status change to Published
/// </summary>
public class PublishExamCommand : IRequest<ExamDto>
{
    public Guid ExamId { get; set; }
}

/// <summary>
/// Command to delete/archive an exam
/// Single Responsibility: Request exam deletion (only when Draft)
/// </summary>
public class DeleteExamCommand : IRequest<bool>
{
    public Guid ExamId { get; set; }
}
