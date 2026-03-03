using MediatR;
using SMS.Application.Features.Exams.DTOs;

namespace SMS.Application.Features.Exams.Queries;

/// <summary>
/// Query to get exam performance analytics
/// Single Responsibility: Request exam analytics data
/// </summary>
public class GetExamAnalyticsQuery : IRequest<ExamAnalyticsDto>
{
    public Guid ExamId { get; set; }
    public Guid? ClassId { get; set; } // Optional: filter to specific class
}

/// <summary>
/// Query to get class performance metrics
/// Single Responsibility: Request class-level performance data
/// </summary>
public class GetClassPerformanceQuery : IRequest<ClassPerformanceDto>
{
    public Guid ExamId { get; set; }
    public Guid ClassId { get; set; }
}

/// <summary>
/// Query to get student performance trend across exams
/// Single Responsibility: Request student's performance history
/// </summary>
public class GetStudentPerformanceTrendQuery : IRequest<StudentPerformanceTrendDto>
{
    public Guid StudentId { get; set; }
}

/// <summary>
/// Query to compare classes in an exam
/// Single Responsibility: Request comparative performance across classes
/// </summary>
public class GetClassComparativeAnalysisQuery : IRequest<ClassComparativeAnalysisDto>
{
    public Guid ExamId { get; set; }
}

/// <summary>
/// Query to get marks distribution histogram data
/// Single Responsibility: Request mark range distribution
/// </summary>
public class GetMarksDistributionQuery : IRequest<MarksDistributionDto>
{
    public Guid ExamId { get; set; }
    public Guid? ClassId { get; set; }
    public int BucketSize { get; set; } = 10; // Default: 10-mark buckets
}

/// <summary>
/// Query to compare exam performance trends in a class
/// Single Responsibility: Request exam-to-exam comparison
/// </summary>
public class GetExamComparisonQuery : IRequest<ExamComparisonAnalysisDto>
{
    public Guid ClassId { get; set; }
    public int? LimitToLastNExams { get; set; } // e.g., 5 = last 5 exams
}

/// <summary>
/// Query to analyze subject performance across exams
/// Single Responsibility: Request subject trend analysis
/// </summary>
public class GetSubjectComparisonQuery : IRequest<SubjectComparisonAnalysisDto>
{
    public Guid SubjectId { get; set; }
    public int? LimitToLastNExams { get; set; }
}

/// <summary>
/// Query to generate comprehensive analytics report
/// Single Responsibility: Request detailed export-ready analytics
/// </summary>
public class GetDetailedAnalyticsReportQuery : IRequest<DetailedAnalyticsReportDto>
{
    public Guid ExamId { get; set; }
    public Guid? ClassId { get; set; }
    public DateTime? ReportPeriodStart { get; set; }
    public DateTime? ReportPeriodEnd { get; set; }
    public bool IncludeStudentDetails { get; set; } = true;
    public bool IncludeSubjectAnalysis { get; set; } = true;
}
