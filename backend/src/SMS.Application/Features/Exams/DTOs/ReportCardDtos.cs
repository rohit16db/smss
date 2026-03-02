namespace SMS.Application.Features.Exams.DTOs;

/// <summary>
/// DTO for report card data
/// Single Responsibility: Transfer report card information
/// </summary>
public class ReportCardDto
{
    public Guid Id { get; set; }
    public Guid ExamId { get; set; }
    public string ExamName { get; set; } = string.Empty;
    public DateTime ExamDate { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string RollNumber { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public List<SubjectReportCardDto> SubjectMarks { get; set; } = new();
    public ReportCardSummaryDto Summary { get; set; } = new();
    public decimal AttendancePercentage { get; set; }
    public string Remarks { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
}

public class SubjectReportCardDto
{
    public Guid SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public decimal MaxMarks { get; set; }
    public decimal Obtained { get; set; }
    public decimal Percentage { get; set; }
    public string Grade { get; set; } = string.Empty;
}

public class ReportCardSummaryDto
{
    public decimal TotalMarks { get; set; }
    public decimal TotalObtained { get; set; }
    public decimal Percentage { get; set; }
    public string OverallGrade { get; set; } = string.Empty;
    public int ClassPosition { get; set; }
    public int TotalStudents { get; set; }
    public string Status { get; set; } = string.Empty; // Pass/Fail
}

/// <summary>
/// DTO for report card list item
/// Single Responsibility: Transfer report card summary for list view
/// </summary>
public class ReportCardListDto
{
    public Guid Id { get; set; }
    public Guid ExamId { get; set; }
    public string ExamName { get; set; } = string.Empty;
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public decimal TotalObtained { get; set; }
    public decimal TotalMarks { get; set; }
    public decimal Percentage { get; set; }
    public string OverallGrade { get; set; } = string.Empty;
    public int ClassPosition { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
}
