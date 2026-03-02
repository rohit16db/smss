namespace SMS.Application.Features.Exams.DTOs;

/// <summary>
/// DTO for exam performance analytics
/// Single Responsibility: Transfer analytics summary data for dashboard
/// </summary>
public class ExamAnalyticsDto
{
    public Guid ExamId { get; set; }
    public string ExamName { get; set; } = string.Empty;
    public DateTime ExamDate { get; set; }
    public int TotalStudents { get; set; }
    public int PassedStudents { get; set; }
    public int FailedStudents { get; set; }
    public decimal PassRate { get; set; } // Percentage: 0-100
    public decimal ClassAverage { get; set; } // Percentage
    public decimal ClassAverageMarks { get; set; }
    public List<GradeDistributionDto> GradeDistribution { get; set; } = new();
    public List<StudentPerformanceDto> TopPerformers { get; set; } = new();
    public List<StudentPerformanceDto> BottomPerformers { get; set; } = new();
    public List<SubjectAnalysisDto> SubjectAnalysis { get; set; } = new();
}

/// <summary>
/// DTO for grade distribution in exam
/// Single Responsibility: Transfer count of students per grade
/// </summary>
public class GradeDistributionDto
{
    public string Grade { get; set; } = string.Empty; // A, B, C, D, F
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}

/// <summary>
/// DTO for individual student performance
/// Single Responsibility: Transfer student's exam performance summary
/// </summary>
public class StudentPerformanceDto
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string RollNumber { get; set; } = string.Empty;
    public decimal MarksObtained { get; set; }
    public decimal Percentage { get; set; }
    public string Grade { get; set; } = string.Empty;
    public int ClassPosition { get; set; }
    public bool Passed { get; set; }
}

/// <summary>
/// DTO for subject-wise exam analysis
/// Single Responsibility: Transfer subject performance metrics
/// </summary>
public class SubjectAnalysisDto
{
    public Guid SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public decimal AverageMarks { get; set; }
    public decimal AveragePercentage { get; set; }
    public decimal HighestMarks { get; set; }
    public decimal LowestMarks { get; set; }
    public int PassCount { get; set; }
    public int FailCount { get; set; }
    public decimal PassPercentage { get; set; }
    public decimal MaxMarks { get; set; }
}

/// <summary>
/// DTO for class performance metrics
/// Single Responsibility: Transfer overall class performance summary
/// </summary>
public class ClassPerformanceDto
{
    public Guid ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public Guid ExamId { get; set; }
    public string ExamName { get; set; } = string.Empty;
    public int TotalEnrolled { get; set; }
    public int AppearedCount { get; set; }
    public int AbsentCount { get; set; }
    public int PassCount { get; set; }
    public int FailCount { get; set; }
    public decimal PassPercentage { get; set; }
    public decimal ClassAverage { get; set; }
    public decimal ClassAveragePercentage { get; set; }
    public decimal HighestMarks { get; set; }
    public decimal LowestMarks { get; set; }
    public List<SubjectAnalysisDto> SubjectWiseAnalysis { get; set; } = new();
    public int StudentsPassed { get; set; }
    public int StudentsFailed { get; set; }
}

/// <summary>
/// DTO for student performance trend across exams
/// Single Responsibility: Transfer student's performance history
/// </summary>
public class StudentPerformanceTrendDto
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string RollNumber { get; set; } = string.Empty;
    public Guid ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public List<ExamPerformancePointDto> PerformanceHistory { get; set; } = new();
    public decimal AveragePercentage { get; set; }
    public decimal LowestPercentage { get; set; }
    public decimal HighestPercentage { get; set; }
    public string PerformanceTrend { get; set; } = string.Empty; // improving, declining, stable
}

/// <summary>
/// DTO for single exam performance point in trend
/// Single Responsibility: Transfer one exam's performance data
/// </summary>
public class ExamPerformancePointDto
{
    public Guid ExamId { get; set; }
    public string ExamName { get; set; } = string.Empty;
    public DateTime ExamDate { get; set; }
    public decimal MarksObtained { get; set; }
    public decimal Percentage { get; set; }
    public string Grade { get; set; } = string.Empty;
    public int ClassPosition { get; set; }
    public bool Passed { get; set; }
}

/// <summary>
/// DTO for comparison across classes
/// Single Responsibility: Transfer comparative performance data
/// </summary>
public class ClassComparativeAnalysisDto
{
    public Guid ExamId { get; set; }
    public string ExamName { get; set; } = string.Empty;
    public List<ClassComparisonItemDto> ClassComparisons { get; set; } = new();
}

/// <summary>
/// DTO for single class in comparative analysis
/// Single Responsibility: Transfer one class's performance metrics
/// </summary>
public class ClassComparisonItemDto
{
    public Guid ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public decimal ClassAverage { get; set; }
    public decimal PassPercentage { get; set; }
    public int EnrolledCount { get; set; }
    public int PassCount { get; set; }
}

/// <summary>
/// DTO for marks distribution (histogram data)
/// Single Responsibility: Transfer mark range distribution
/// </summary>
public class MarksDistributionDto
{
    public Guid ExamId { get; set; }
    public List<MarkRangeBucketDto> Buckets { get; set; } = new();
    public decimal Total { get; set; }
}

/// <summary>
/// DTO for single bucket in marks distribution
/// Single Responsibility: Transfer count of students in mark range
/// </summary>
public class MarkRangeBucketDto
{
    public string RangeLabel { get; set; } = string.Empty; // e.g., "0-10", "10-20", etc.
    public int StartMark { get; set; }
    public int EndMark { get; set; }
    public int StudentCount { get; set; }
    public decimal Percentage { get; set; }
}

/// <summary>
/// DTO for exam comparison analysis
/// Single Responsibility: Transfer exam-to-exam comparison data
/// </summary>
public class ExamComparisonAnalysisDto
{
    public Guid ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public List<ExamComparisonItemDto> ExamComparisons { get; set; } = new();
}

/// <summary>
/// DTO for single exam in comparison
/// Single Responsibility: Transfer one exam's comparative metrics
/// </summary>
public class ExamComparisonItemDto
{
    public Guid ExamId { get; set; }
    public string ExamName { get; set; } = string.Empty;
    public DateTime ExamDate { get; set; }
    public decimal ClassAverage { get; set; }
    public decimal PassPercentage { get; set; }
    public int PassCount { get; set; }
    public int TotalStudents { get; set; }
}

/// <summary>
/// DTO for subject comparison across exams
/// Single Responsibility: Transfer subject's performance across exams
/// </summary>
public class SubjectComparisonAnalysisDto
{
    public Guid SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public List<SubjectExamComparisonDto> ExamPerformance { get; set; } = new();
}

/// <summary>
/// DTO for subject performance in single exam
/// Single Responsibility: Transfer subject metrics for one exam
/// </summary>
public class SubjectExamComparisonDto
{
    public Guid ExamId { get; set; }
    public string ExamName { get; set; } = string.Empty;
    public DateTime ExamDate { get; set; }
    public decimal AverageMarks { get; set; }
    public decimal AveragePercentage { get; set; }
    public int PassCount { get; set; }
    public int FailCount { get; set; }
}

/// <summary>
/// DTO for detailed analytics export report
/// Single Responsibility: Transfer comprehensive analytics data for export
/// </summary>
public class DetailedAnalyticsReportDto
{
    public string ReportTitle { get; set; } = "Exam Performance Report";
    public DateTime GeneratedDate { get; set; }
    public Guid ExamId { get; set; }
    public string ExamName { get; set; } = string.Empty;
    public Guid? ClassId { get; set; }
    public string? ClassName { get; set; }
    public DateTime ReportPeriodStart { get; set; }
    public DateTime ReportPeriodEnd { get; set; }
    
    // Summary metrics
    public int TotalStudents { get; set; }
    public int StudentsAppeared { get; set; }
    public int StudentsAbsent { get; set; }
    public decimal OverallPassPercentage { get; set; }
    public decimal OverallClassAverage { get; set; }
    
    // Detailed data
    public ExamAnalyticsDto? ExamAnalytics { get; set; }
    public ClassPerformanceDto? ClassPerformance { get; set; }
    public List<StudentPerformanceDto> AllStudentPerformance { get; set; } = new();
    public List<SubjectAnalysisDto> AllSubjectsAnalysis { get; set; } = new();
    public MarksDistributionDto? MarksDistribution { get; set; }
    public List<ExamComparisonItemDto> ExamTrendData { get; set; } = new();
}
