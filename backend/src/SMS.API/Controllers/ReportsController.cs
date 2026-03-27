using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMS.Application.Features.Reports.DTOs;
using SMS.Application.Features.Reports.Queries;

namespace SMS.API.Controllers;

/// <summary>
/// API endpoints for fee collection and payment reports
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FeeReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public FeeReportsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get fee collection summary for a date range
    /// </summary>
    /// <param name="startDate">Start date (YYYY-MM-DD)</param>
    /// <param name="endDate">End date (YYYY-MM-DD)</param>
    /// <param name="category">Optional fee category filter</param>
    /// <param name="prevStartDate">Optional previous period start date</param>
    /// <param name="prevEndDate">Optional previous period end date</param>
    /// <returns>Fee collection summary with statistics</returns>
    [HttpGet("collection-summary")]
    [Authorize(Policy = "FeesAccess")]
    [ProducesResponseType(typeof(FeeCollectionSummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<FeeCollectionSummaryDto>> GetCollectionSummary(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] string? category = null,
        [FromQuery] DateTime? prevStartDate = null,
        [FromQuery] DateTime? prevEndDate = null)
    {
        var query = new GetFeeCollectionSummaryQuery(startDate, endDate)
        {
            Category = category,
            PreviousPeriodStartDate = prevStartDate,
            PreviousPeriodEndDate = prevEndDate
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get monthly fee collection trend
    /// </summary>
    /// <param name="startDate">Start date (YYYY-MM-DD)</param>
    /// <param name="endDate">End date (YYYY-MM-DD)</param>
    /// <param name="category">Optional fee category filter</param>
    /// <returns>Monthly collection trend data</returns>
    [HttpGet("monthly-trend")]
    [Authorize(Policy = "FeesAccess")]
    [ProducesResponseType(typeof(IEnumerable<MonthlyCollectionTrendDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MonthlyCollectionTrendDto>>> GetMonthlyTrend(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] string? category = null)
    {
        var query = new GetMonthlyFeeCollectionTrendQuery(startDate, endDate)
        {
            Category = category
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get fee collection breakdown by category
    /// </summary>
    /// <param name="startDate">Start date (YYYY-MM-DD)</param>
    /// <param name="endDate">End date (YYYY-MM-DD)</param>
    /// <returns>Fee breakdown by category</returns>
    [HttpGet("by-category")]
    [Authorize(Policy = "FeesAccess")]
    [ProducesResponseType(typeof(IEnumerable<FeeCollectionByCategoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<FeeCollectionByCategoryDto>>> GetByCategory(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var query = new GetFeeCollectionByCategoryQuery(startDate, endDate);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get outstanding and overdue fees with aging analysis
    /// </summary>
    /// <param name="asOfDate">Analysis date (defaults to today)</param>
    /// <param name="agingBucket">Optional aging bucket filter: 0-30, 31-60, 61-90, 90+</param>
    /// <param name="minAmount">Optional minimum due amount filter</param>
    /// <param name="sortBy">Sort field: daysoverdue (default), dueamount, name, class</param>
    /// <param name="descending">Descending sort order (default true)</param>
    /// <returns>List of outstanding fees</returns>
    [HttpGet("outstanding")]
    [Authorize(Policy = "FeesAccess")]
    [ProducesResponseType(typeof(IEnumerable<OutstandingFeeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OutstandingFeeDto>>> GetOutstanding(
        [FromQuery] DateTime? asOfDate = null,
        [FromQuery] string? agingBucket = null,
        [FromQuery] decimal? minAmount = null,
        [FromQuery] string sortBy = "daysoverdue",
        [FromQuery] bool descending = true)
    {
        var query = new GetOutstandingFeesQuery(asOfDate ?? DateTime.UtcNow)
        {
            AgingBucket = agingBucket,
            MinimumDueAmount = minAmount,
            SortBy = sortBy,
            Descending = descending
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get payment history for a specific student
    /// </summary>
    /// <param name="studentId">Student ID (required)</param>
    /// <param name="startDate">Start date (YYYY-MM-DD)</param>
    /// <param name="endDate">End date (YYYY-MM-DD)</param>
    /// <returns>Student payment history</returns>
    [HttpGet("student/{studentId}/payment-history")]
    [Authorize(Policy = "FeesAccess")]
    [ProducesResponseType(typeof(IEnumerable<StudentPaymentHistoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<StudentPaymentHistoryDto>>> GetStudentPaymentHistory(
        [FromRoute] string studentId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var query = new GetStudentPaymentHistoryQuery(studentId, startDate, endDate);
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}

/// <summary>
/// API endpoints for salary expense and payroll reports
/// </summary>
[ApiController]
[Route("api/salary-reports")]
[Authorize]
public class SalaryReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SalaryReportsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get salary expense summary for a date range
    /// </summary>
    /// <param name="startDate">Start date (YYYY-MM-DD)</param>
    /// <param name="endDate">End date (YYYY-MM-DD)</param>
    /// <param name="prevStartDate">Optional previous period start date for comparison</param>
    /// <param name="prevEndDate">Optional previous period end date for comparison</param>
    /// <returns>Salary expense summary with statistics</returns>
    [HttpGet("expense-summary")]
    [Authorize(Policy = "SalaryAccess")]
    [ProducesResponseType(typeof(SalaryExpenseSummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SalaryExpenseSummaryDto>> GetExpenseSummary(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] DateTime? prevStartDate = null,
        [FromQuery] DateTime? prevEndDate = null)
    {
        var query = new GetSalaryExpenseSummaryQuery(startDate, endDate)
        {
            PreviousPeriodStartDate = prevStartDate,
            PreviousPeriodEndDate = prevEndDate
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get monthly salary expense trend
    /// </summary>
    /// <param name="startDate">Start date (YYYY-MM-DD)</param>
    /// <param name="endDate">End date (YYYY-MM-DD)</param>
    /// <returns>Monthly salary expense trend</returns>
    [HttpGet("monthly-trend")]
    [Authorize(Policy = "SalaryAccess")]
    [ProducesResponseType(typeof(IEnumerable<MonthlySalaryTrendDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MonthlySalaryTrendDto>>> GetMonthlyTrend(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var query = new GetMonthlySalaryTrendQuery(startDate, endDate);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get salary component breakdown (Base, Bonus, Deductions)
    /// </summary>
    /// <param name="startDate">Start date (YYYY-MM-DD)</param>
    /// <param name="endDate">End date (YYYY-MM-DD)</param>
    /// <returns>Component breakdown with percentages</returns>
    [HttpGet("component-breakdown")]
    [Authorize(Policy = "SalaryAccess")]
    [ProducesResponseType(typeof(SalaryComponentBreakdownDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SalaryComponentBreakdownDto>> GetComponentBreakdown(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var query = new GetSalaryComponentBreakdownQuery(startDate, endDate);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get staff-wise salary comparison
    /// </summary>
    /// <param name="startDate">Start date (YYYY-MM-DD)</param>
    /// <param name="endDate">End date (YYYY-MM-DD)</param>
    /// <param name="status">Optional status filter: Pending, Approved, Paid</param>
    /// <param name="sortBy">Sort field: name (default), netsalary, bonus, deduction</param>
    /// <param name="descending">Descending sort order</param>
    /// <returns>Staff salary comparison list</returns>
    [HttpGet("staff-comparison")]
    [Authorize(Policy = "SalaryAccess")]
    [ProducesResponseType(typeof(IEnumerable<StaffSalaryComparisonDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<StaffSalaryComparisonDto>>> GetStaffComparison(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] string? status = null,
        [FromQuery] string sortBy = "name",
        [FromQuery] bool descending = false)
    {
        var query = new GetStaffSalaryComparisonQuery(startDate, endDate)
        {
            Status = status,
            SortBy = sortBy,
            Descending = descending
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get attendance to salary correlation analysis
    /// </summary>
    /// <param name="month">Analysis month (YYYY-MM-DD format)</param>
    /// <param name="onlyDiscrepancies">Show only discrepancies between calculated and actual deductions</param>
    /// <returns>Attendance to salary correlation data</returns>
    [HttpGet("attendance-correlation")]
    [Authorize(Policy = "SalaryAccess")]
    [ProducesResponseType(typeof(IEnumerable<AttendanceToSalaryCorrelationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AttendanceToSalaryCorrelationDto>>> GetAttendanceCorrelation(
        [FromQuery] DateTime month,
        [FromQuery] bool onlyDiscrepancies = false)
    {
        var query = new GetAttendanceToSalaryCorrelationQuery(month)
        {
            OnlyDiscrepancies = onlyDiscrepancies
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get budget vs actual comparison
    /// </summary>
    /// <param name="reportType">Report type: FeeCollection or SalaryExpense (required)</param>
    /// <param name="startDate">Start date (YYYY-MM-DD)</param>
    /// <param name="endDate">End date (YYYY-MM-DD)</param>
    /// <param name="groupBy">Group by: month (default), category, class</param>
    /// <returns>Budget vs actual data</returns>
    [HttpGet("budget-vs-actual")]
    [Authorize(Policy = "SalaryAccess")]
    [ProducesResponseType(typeof(IEnumerable<BudgetVsActualDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<BudgetVsActualDto>>> GetBudgetVsActual(
        [FromQuery] string reportType,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] string? groupBy = "month")
    {
        var query = new GetBudgetVsActualQuery(reportType, startDate, endDate)
        {
            GroupBy = groupBy
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
