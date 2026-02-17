namespace SMS.Application.Features.Dashboard.DTOs;

/// <summary>
/// Summary card data for dashboard display
/// </summary>
public class DashboardSummaryCardDto
{
    public string Title { get; set; }
    public decimal Value { get; set; }
    public string? Unit { get; set; }
    public decimal? PercentageChange { get; set; }
    public string? IconName { get; set; }
    public string? TrendDirection { get; set; } // "up", "down", "stable"
}

/// <summary>
/// Financial summary data
/// </summary>
public class FinancialSummaryDto
{
    public decimal TotalFeesCollected { get; set; }
    public decimal TotalOutstandingFees { get; set; }
    public decimal TotalExpectedFees { get; set; }
    public decimal CollectionPercentage { get; set; }
    public int TotalStudents { get; set; }
    public decimal AveragePaymentPerStudent { get; set; }
}

/// <summary>
/// Attendance summary data
/// </summary>
public class AttendanceSummaryDto
{
    public decimal AverageStudentAttendance { get; set; }
    public decimal AverageTeacherAttendance { get; set; }
    public int TotalTeachers { get; set; }
    public int TotalStudents { get; set; }
    public int PresentStudentsTodayCount { get; set; }
    public int AbsentStudentsTodayCount { get; set; }
}

/// <summary>
/// Academic summary data
/// </summary>
public class AcademicSummaryDto
{
    public int TotalStudents { get; set; }
    public int TotalTeachers { get; set; }
    public int TotalClasses { get; set; }
    public int ActiveStudents { get; set; }
    public int ActiveTeachers { get; set; }
}

/// <summary>
/// Complete dashboard summary response
/// </summary>
public class DashboardSummaryResponseDto
{
    public DateTime GeneratedAt { get; set; }
    public AcademicSummaryDto AcademicSummary { get; set; }
    public FinancialSummaryDto FinancialSummary { get; set; }
    public AttendanceSummaryDto AttendanceSummary { get; set; }
    public List<DashboardSummaryCardDto> SummaryCards { get; set; }
}

/// <summary>
/// Fee collection trend data for charts
/// </summary>
public class FeeCollectionTrendDto
{
    public DateTime Date { get; set; }
    public decimal CollectedAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public decimal TargetAmount { get; set; }
}

/// <summary>
/// Attendance trend data for charts
/// </summary>
public class AttendanceTrendDto
{
    public DateTime Date { get; set; }
    public decimal StudentAttendancePercentage { get; set; }
    public decimal TeacherAttendancePercentage { get; set; }
}

/// <summary>
/// Outstanding fees by student
/// </summary>
public class OutstandingFeeDetailDto
{
    public string StudentId { get; set; }
    public string StudentName { get; set; }
    public decimal OutstandingAmount { get; set; }
    public DateTime DueDate { get; set; }
    public int DaysOverdue { get; set; }
    public string Status { get; set; } // "Due Soon", "Overdue", "Severely Overdue"
}
