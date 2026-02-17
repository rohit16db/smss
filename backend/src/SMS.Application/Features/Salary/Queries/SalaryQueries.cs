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
/// Get salary payments for a specific teacher
/// </summary>
public class GetTeacherSalaryPaymentsQuery : IRequest<SalaryHistoryDto>
{
    public Guid TeacherId { get; set; }
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
/// Get salary payment summary for dashboard
/// </summary>
public class GetSalarySummaryQuery : IRequest<SalarySummaryDto>
{
    public int? Month { get; set; }
    public int? Year { get; set; }
}

/// <summary>
/// DTO for salary summary
/// </summary>
public class SalarySummaryDto
{
    public decimal TotalSalaryExpense { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal TotalPending { get; set; }
    public int TeacherCount { get; set; }
    public int PaidCount { get; set; }
    public int PendingCount { get; set; }
    public decimal AverageSalaryPerTeacher { get; set; }
}
