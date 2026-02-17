using System.ComponentModel.DataAnnotations;

namespace SMS.Application.Features.Salary.DTOs;

/// <summary>
/// DTO for creating or updating a salary payment
/// </summary>
public class CreateSalaryPaymentDto
{
    [Required(ErrorMessage = "Teacher ID is required")]
    public Guid TeacherId { get; set; }

    [Required(ErrorMessage = "Period start date is required")]
    public DateOnly PeriodStartDate { get; set; }

    [Required(ErrorMessage = "Period end date is required")]
    public DateOnly PeriodEndDate { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Base salary must be greater than 0")]
    public decimal BaseSalary { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Deductions cannot be negative")]
    public decimal Deductions { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Bonus cannot be negative")]
    public decimal Bonus { get; set; }

    [StringLength(100, ErrorMessage = "Reference number cannot exceed 100 characters")]
    public string? ReferenceNumber { get; set; }

    [StringLength(50, ErrorMessage = "Payment method cannot exceed 50 characters")]
    public string? PaymentMethod { get; set; }

    [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters")]
    public string? Remarks { get; set; }
}

/// <summary>
/// DTO for updating salary payment status
/// </summary>
public class UpdateSalaryPaymentStatusDto
{
    [Required(ErrorMessage = "Status is required")]
    [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
    public string Status { get; set; } = string.Empty;

    public DateOnly? PaidDate { get; set; }

    [StringLength(100, ErrorMessage = "Reference number cannot exceed 100 characters")]
    public string? ReferenceNumber { get; set; }

    [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters")]
    public string? Remarks { get; set; }
}

/// <summary>
/// DTO for salary payment response
/// </summary>
public class SalaryPaymentDto
{
    public Guid Id { get; set; }
    public Guid TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public DateOnly PeriodStartDate { get; set; }
    public DateOnly PeriodEndDate { get; set; }
    public decimal BaseSalary { get; set; }
    public decimal Deductions { get; set; }
    public decimal Bonus { get; set; }
    public decimal NetSalary { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateOnly? PaidDate { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Remarks { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// DTO for salary payment report
/// </summary>
public class SalaryPaymentReportDto
{
    public DateOnly MonthStart { get; set; }
    public DateOnly MonthEnd { get; set; }
    public int TotalTeachers { get; set; }
    public int PaidTeachers { get; set; }
    public int PendingTeachers { get; set; }
    public decimal TotalBaseSalary { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TotalBonus { get; set; }
    public decimal TotalNetSalary { get; set; }
    public List<SalaryPaymentDto> PaymentDetails { get; set; } = new();
}

/// <summary>
/// DTO for teacher salary history
/// </summary>
public class SalaryHistoryDto
{
    public Guid TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public List<SalaryPaymentDto> PaymentHistory { get; set; } = new();
    public decimal TotalSalaryPaid { get; set; }
    public decimal AverageMonthlySalary { get; set; }
    public int TotalPayments { get; set; }
    public int PendingPayments { get; set; }
}

/// <summary>
/// DTO for salary payment summary statistics
/// </summary>
public class SalaryPaymentSummaryDto
{
    public int TotalPayments { get; set; }
    public int PendingCount { get; set; }
    public int ApprovedCount { get; set; }
    public int PaidCount { get; set; }
    public int OnHoldCount { get; set; }
    public int CancelledCount { get; set; }
    public decimal TotalBaseSalary { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TotalBonus { get; set; }
    public decimal TotalNetSalary { get; set; }
    public decimal TotalPaidAmount { get; set; }
}

/// <summary>
/// DTO for marking salary as paid
/// </summary>
public class MarkSalaryAsPaidDto
{
    public DateOnly PaidDate { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string? PaymentMethod { get; set; }
}

/// <summary>
/// DTO for updating salary payment details
/// </summary>
public class UpdateSalaryPaymentDto
{
    public decimal? BaseSalary { get; set; }
    public decimal? Deductions { get; set; }
    public decimal? Bonus { get; set; }
    public string? Remarks { get; set; }
}

