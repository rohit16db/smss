namespace SMS.Domain.Entities;

/// <summary>
/// Represents a salary payment record for a teacher
/// </summary>
public class SalaryPayment
{
    public Guid Id { get; set; }
    public Guid TeacherId { get; set; }
    public Teacher Teacher { get; set; } = null!;

    /// <summary>
    /// Salary period start date
    /// </summary>
    public DateOnly PeriodStartDate { get; set; }

    /// <summary>
    /// Salary period end date
    /// </summary>
    public DateOnly PeriodEndDate { get; set; }

    /// <summary>
    /// Base salary for the period
    /// </summary>
    public decimal BaseSalary { get; set; }

    /// <summary>
    /// Deductions (e.g., absence, late, disciplinary)
    /// </summary>
    public decimal Deductions { get; set; }

    /// <summary>
    /// Bonus amount (if any)
    /// </summary>
    public decimal Bonus { get; set; }

    /// <summary>
    /// Net salary = BaseSalary - Deductions + Bonus
    /// </summary>
    public decimal NetSalary { get; set; }

    /// <summary>
    /// Salary payment status
    /// </summary>
    public SalaryPaymentStatus Status { get; set; } = SalaryPaymentStatus.Pending;

    /// <summary>
    /// Date when salary was actually paid
    /// </summary>
    public DateOnly? PaidDate { get; set; }

    /// <summary>
    /// Reference/Check number for payment tracking
    /// </summary>
    public string? ReferenceNumber { get; set; }

    /// <summary>
    /// Payment method (Cash, Bank Transfer, Cheque, etc.)
    /// </summary>
    public PaymentMethod? PaymentMethod { get; set; }

    /// <summary>
    /// Remarks/Notes about the payment
    /// </summary>
    public string? Remarks { get; set; }

    /// <summary>
    /// Audit trail
    /// </summary>
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public enum SalaryPaymentStatus
{
    Pending = 0,
    Approved = 1,
    Paid = 2,
    Cancelled = 3,
    OnHold = 4
}

public enum PaymentMethod
{
    Cash = 0,
    BankTransfer = 1,
    Cheque = 2,
    MobilePayment = 3,
    Other = 4
}
