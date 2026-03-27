using MediatR;
using SMS.Application.Features.Salary.DTOs;

namespace SMS.Application.Features.Salary.Queries;

/// <summary>
/// Get salary payment by ID
/// </summary>
public class GetSalaryPaymentQuery : IRequest<SalaryPaymentDto>
{
    public Guid SalaryPaymentId { get; set; }
}

/// <summary>
/// Get all salary payments for a specific month/period
/// </summary>
public class GetSalaryPaymentsByPeriodQuery : IRequest<SalaryPaymentReportDto>
{
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
}

/// <summary>
/// Get salary payments for a specific staff member
/// </summary>
public class GetStaffSalaryPaymentsQuery : IRequest<SalaryHistoryDto>
{
    public Guid StaffId { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}

/// <summary>
/// Get all pending salary payments
/// </summary>
public class GetPendingSalaryPaymentsQuery : IRequest<List<SalaryPaymentDto>>
{
    public DateOnly? AsOfDate { get; set; }
}

/// <summary>
/// Get staff salary summary for dashboard
/// </summary>
public class GetStaffSalarySummaryQuery : IRequest<StaffSalarySummaryDto>
{
    public int? Month { get; set; }
    public int? Year { get; set; }
}

/// <summary>
/// DTO for salary summary
/// </summary>
public class StaffSalarySummaryDto
{
    public decimal TotalSalaryExpense { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal TotalPending { get; set; }
    public int StaffCount { get; set; }
    public int PaidCount { get; set; }
    public int PendingCount { get; set; }
    public decimal AverageSalaryPerStaff { get; set; }
}

/// <summary>
/// Get all salary payments with optional filters
/// </summary>
public class GetAllSalaryPaymentsQuery : IRequest<List<SalaryPaymentDto>>
{
    public string? Status { get; set; }
    public Guid? StaffId { get; set; }
    public DateTime? PeriodStartDate { get; set; }
    public DateTime? PeriodEndDate { get; set; }
}

/// <summary>
/// Get salary payments summary statistics
/// </summary>
public class GetSalaryPaymentsSummaryQuery : IRequest<SalaryPaymentSummaryDto>
{
    public DateTime? PeriodStartDate { get; set; }
    public DateTime? PeriodEndDate { get; set; }
}
