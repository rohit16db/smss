using MediatR;
using SMS.Application.Features.Salary.DTOs;

namespace SMS.Application.Features.Salary.Commands;

/// <summary>
/// Create a new salary payment record
/// </summary>
public class CreateSalaryPaymentCommand : IRequest<SalaryPaymentDto>
{
    public Guid StaffId { get; set; }
    public DateOnly PeriodStartDate { get; set; }
    public DateOnly PeriodEndDate { get; set; }
    public decimal BaseSalary { get; set; }
    public decimal Deductions { get; set; }
    public decimal Bonus { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Remarks { get; set; }
}

/// <summary>
/// Update salary payment status (approve, pay, cancel, etc.)
/// </summary>
public class UpdateSalaryPaymentStatusCommand : IRequest<SalaryPaymentDto>
{
    public Guid SalaryPaymentId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateOnly? PaidDate { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Remarks { get; set; }
}

/// <summary>
/// Mark salary payment as paid
/// </summary>
public class MarkSalaryAsPaidCommand : IRequest<SalaryPaymentDto>
{
    public Guid SalaryPaymentId { get; set; }
    public DateOnly PaidDate { get; set; }
    public string? PaymentMethod { get; set; }
    public string? ReferenceNumber { get; set; }
}

/// <summary>
/// Bulk create salary payments for all active staff
/// </summary>
public class CreateBulkSalaryPaymentsCommand : IRequest<SalaryPaymentReportDto>
{
    public DateOnly PeriodStartDate { get; set; }
    public DateOnly PeriodEndDate { get; set; }
    public Dictionary<Guid, decimal> BaseSalariesByStaffId { get; set; } = new();
    public Dictionary<Guid, decimal> DeductionsByStaffId { get; set; } = new();
    public Dictionary<Guid, decimal> BonusesByStaffId { get; set; } = new();
}

/// <summary>
/// Delete a salary payment (only if not paid)
/// </summary>
public class DeleteSalaryPaymentCommand : IRequest<bool>
{
    public Guid SalaryPaymentId { get; set; }
}

/// <summary>
/// Update salary payment amounts (base salary, deductions, bonus)
/// </summary>
public class UpdateSalaryPaymentCommand : IRequest<SalaryPaymentDto>
{
    public Guid SalaryPaymentId { get; set; }
    public decimal? BaseSalary { get; set; }
    public decimal? Deductions { get; set; }
    public decimal? Bonus { get; set; }
    public string? Remarks { get; set; }
}
