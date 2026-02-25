using MediatR;
using SMS.Application.Features.Reports.DTOs;

namespace SMS.Application.Features.Reports.Queries;

/// <summary>
/// Query for fee collection summary and statistics
/// </summary>
public class GetFeeCollectionSummaryQuery : IRequest<FeeCollectionSummaryDto>
{
    /// <summary>Start date for the date range (inclusive)</summary>
    public DateTime StartDate { get; set; }

    /// <summary>End date for the date range (inclusive)</summary>
    public DateTime EndDate { get; set; }

    /// <summary>Optional start date for previous period comparison</summary>
    public DateTime? PreviousPeriodStartDate { get; set; }

    /// <summary>Optional end date for previous period comparison</summary>
    public DateTime? PreviousPeriodEndDate { get; set; }

    /// <summary>Optional fee category filter</summary>
    public string? Category { get; set; }

    /// <summary>Optional student class filter</summary>
    public int? ClassId { get; set; }

    public GetFeeCollectionSummaryQuery(DateTime startDate, DateTime endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }
}

/// <summary>
/// Query for monthly fee collection trend analysis
/// </summary>
public class GetMonthlyFeeCollectionTrendQuery : IRequest<IEnumerable<MonthlyCollectionTrendDto>>
{
    /// <summary>Start month (YYYY-MM format or DateTime)</summary>
    public DateTime StartDate { get; set; }

    /// <summary>End month (YYYY-MM format or DateTime)</summary>
    public DateTime EndDate { get; set; }

    /// <summary>Optional fee category filter</summary>
    public string? Category { get; set; }

    /// <summary>Optional student class filter</summary>
    public int? ClassId { get; set; }

    public GetMonthlyFeeCollectionTrendQuery(DateTime startDate, DateTime endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }
}

/// <summary>
/// Query for fee collection breakdown by category
/// </summary>
public class GetFeeCollectionByCategoryQuery : IRequest<IEnumerable<FeeCollectionByCategoryDto>>
{
    /// <summary>Start date for the date range (inclusive)</summary>
    public DateTime StartDate { get; set; }

    /// <summary>End date for the date range (inclusive)</summary>
    public DateTime EndDate { get; set; }

    /// <summary>Optional student class filter</summary>
    public int? ClassId { get; set; }

    public GetFeeCollectionByCategoryQuery(DateTime startDate, DateTime endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }
}

/// <summary>
/// Query for outstanding/overdue fees analysis with aging report
/// </summary>
public class GetOutstandingFeesQuery : IRequest<IEnumerable<OutstandingFeeDto>>
{
    /// <summary>As of date for outstanding analysis</summary>
    public DateTime AsOfDate { get; set; }

    /// <summary>Optional aging bucket filter: "0-30", "31-60", "61-90", "90+"</summary>
    public string? AgingBucket { get; set; }

    /// <summary>Optional student class filter</summary>
    public int? ClassId { get; set; }

    /// <summary>Optional minimum due amount filter (e.g., only show > 1000)</summary>
    public decimal? MinimumDueAmount { get; set; }

    /// <summary>Sort by: "daysoverdue" (default), "dueamount", "name", "class"</summary>
    public string? SortBy { get; set; } = "daysoverdue";

    /// <summary>Descending order (default true)</summary>
    public bool Descending { get; set; } = true;

    public GetOutstandingFeesQuery()
    {
        AsOfDate = DateTime.UtcNow;
    }

    public GetOutstandingFeesQuery(DateTime asOfDate)
    {
        AsOfDate = asOfDate;
    }
}

/// <summary>
/// Query for individual student payment history
/// </summary>
public class GetStudentPaymentHistoryQuery : IRequest<IEnumerable<StudentPaymentHistoryDto>>
{
    /// <summary>Student ID (required)</summary>
    public string StudentId { get; set; }

    /// <summary>Start date for history</summary>
    public DateTime StartDate { get; set; }

    /// <summary>End date for history</summary>
    public DateTime EndDate { get; set; }

    public GetStudentPaymentHistoryQuery(string studentId, DateTime startDate, DateTime endDate)
    {
        StudentId = studentId;
        StartDate = startDate;
        EndDate = endDate;
    }
}

/// <summary>
/// Query for salary expense summary and statistics
/// </summary>
public class GetSalaryExpenseSummaryQuery : IRequest<SalaryExpenseSummaryDto>
{
    /// <summary>Start date for the date range (inclusive)</summary>
    public DateTime StartDate { get; set; }

    /// <summary>End date for the date range (inclusive)</summary>
    public DateTime EndDate { get; set; }

    /// <summary>Optional start date for previous period comparison</summary>
    public DateTime? PreviousPeriodStartDate { get; set; }

    /// <summary>Optional end date for previous period comparison</summary>
    public DateTime? PreviousPeriodEndDate { get; set; }

    /// <summary>Optional salary structure filter (e.g., "PrincipalStructure")</summary>
    public string? SalaryStructure { get; set; }

    public GetSalaryExpenseSummaryQuery(DateTime startDate, DateTime endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }
}

/// <summary>
/// Query for monthly salary expense trend analysis
/// </summary>
public class GetMonthlySalaryTrendQuery : IRequest<IEnumerable<MonthlySalaryTrendDto>>
{
    /// <summary>Start month (YYYY-MM format or DateTime)</summary>
    public DateTime StartDate { get; set; }

    /// <summary>End month (YYYY-MM format or DateTime)</summary>
    public DateTime EndDate { get; set; }

    /// <summary>Optional salary structure filter</summary>
    public string? SalaryStructure { get; set; }

    public GetMonthlySalaryTrendQuery(DateTime startDate, DateTime endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }
}

/// <summary>
/// Query for salary component breakdown
/// </summary>
public class GetSalaryComponentBreakdownQuery : IRequest<SalaryComponentBreakdownDto>
{
    /// <summary>Start date for the date range (inclusive)</summary>
    public DateTime StartDate { get; set; }

    /// <summary>End date for the date range (inclusive)</summary>
    public DateTime EndDate { get; set; }

    /// <summary>Optional salary structure filter</summary>
    public string? SalaryStructure { get; set; }

    public GetSalaryComponentBreakdownQuery(DateTime startDate, DateTime endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }
}

/// <summary>
/// Query for teacher-wise salary comparison and analysis
/// </summary>
public class GetTeacherSalaryComparisonQuery : IRequest<IEnumerable<TeacherSalaryComparisonDto>>
{
    /// <summary>Start date for salary data</summary>
    public DateTime StartDate { get; set; }

    /// <summary>End date for salary data</summary>
    public DateTime EndDate { get; set; }

    /// <summary>Optional salary structure filter</summary>
    public string? SalaryStructure { get; set; }

    /// <summary>Optional salary status filter: "Pending", "Approved", "Paid"</summary>
    public string? Status { get; set; }

    /// <summary>Sort by: "name" (default), "netsalary", "bonus", "deduction"</summary>
    public string? SortBy { get; set; } = "name";

    /// <summary>Descending order (default false for name, true for amounts)</summary>
    public bool Descending { get; set; } = false;

    public GetTeacherSalaryComparisonQuery(DateTime startDate, DateTime endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }
}

/// <summary>
/// Query for attendance to salary correlation analysis
/// </summary>
public class GetAttendanceToSalaryCorrelationQuery : IRequest<IEnumerable<AttendanceToSalaryCorrelationDto>>
{
    /// <summary>Month for analysis (YYYY-MM or DateTime)</summary>
    public DateTime Month { get; set; }

    /// <summary>Optional salary structure filter</summary>
    public string? SalaryStructure { get; set; }

    /// <summary>Flag to show only discrepancies</summary>
    public bool OnlyDiscrepancies { get; set; } = false;

    public GetAttendanceToSalaryCorrelationQuery(DateTime month)
    {
        Month = month;
    }
}

/// <summary>
/// Query for budget vs actual comparison for fee collection or salary expenses
/// </summary>
public class GetBudgetVsActualQuery : IRequest<IEnumerable<BudgetVsActualDto>>
{
    /// <summary>Report type: "FeeCollection" or "SalaryExpense"</summary>
    public string ReportType { get; set; }

    /// <summary>Start date for the period</summary>
    public DateTime StartDate { get; set; }

    /// <summary>End date for the period</summary>
    public DateTime EndDate { get; set; }

    /// <summary>Group by: "month" (default), "category", "class"</summary>
    public string? GroupBy { get; set; } = "month";

    public GetBudgetVsActualQuery(string reportType, DateTime startDate, DateTime endDate)
    {
        ReportType = reportType;
        StartDate = startDate;
        EndDate = endDate;
    }
}
