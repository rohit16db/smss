using MediatR;
using SMS.Application.Features.Exams.DTOs;

namespace SMS.Application.Features.Exams.Queries;

/// <summary>
/// Query to get marks entry form for a class
/// Single Responsibility: Request marks form with all students
/// </summary>
public class GetMarksEntryFormQuery : IRequest<MarksEntryFormDto>
{
    public Guid ExamId { get; set; }
    public Guid ClassId { get; set; }
    public Guid SectionId { get; set; }
    public string SortBy { get; set; } = "rollNumber"; // rollNumber, name
    public string SortOrder { get; set; } = "asc";
}

/// <summary>
/// Query to get marks for a single student
/// Single Responsibility: Request student marks for specific exam
/// </summary>
public class GetStudentMarksQuery : IRequest<StudentMarksDto>
{
    public Guid ExamId { get; set; }
    public Guid StudentId { get; set; }
}

/// <summary>
/// Query to get all marks for a class
/// Single Responsibility: Request all student marks for class
/// </summary>
public class GetClassMarksQuery : IRequest<List<StudentMarksDto>>
{
    public Guid ExamId { get; set; }
    public Guid ClassId { get; set; }
}
