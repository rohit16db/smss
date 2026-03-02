using MediatR;
using SMS.Application.Features.Exams.DTOs;

namespace SMS.Application.Features.Exams.Queries;

/// <summary>
/// Query to get a specific report card
/// Single Responsibility: Request report card for student
/// </summary>
public class GetReportCardQuery : IRequest<ReportCardDto>
{
    public Guid ExamId { get; set; }
    public Guid StudentId { get; set; }
}

/// <summary>
/// Query to get all report cards for an exam
/// Single Responsibility: Request report cards with filtering
/// </summary>
public class GetExamReportCardsQuery : IRequest<List<ReportCardListDto>>
{
    public Guid ExamId { get; set; }
    public Guid? ClassId { get; set; }
    public string? Status { get; set; } // pass, fail
    public string SortBy { get; set; } = "classPosition"; // classPosition, name, percentage
    public string SortOrder { get; set; } = "asc";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Query to get all report cards for a student
/// Single Responsibility: Request student's all report cards
/// </summary>
public class GetStudentReportCardsQuery : IRequest<List<ReportCardListDto>>
{
    public Guid StudentId { get; set; }
}

/// <summary>
/// Query to export report card as PDF
/// Single Responsibility: Request PDF export of report card
/// </summary>
public class ExportReportCardPdfQuery : IRequest<byte[]>
{
    public Guid CardId { get; set; }
}
