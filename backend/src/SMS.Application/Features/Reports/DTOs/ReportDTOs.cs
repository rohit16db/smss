namespace SMS.Application.Features.Reports.DTOs;

/// <summary>
/// Fee collection summary statistics
/// </summary>
public class FeeCollectionSummaryDto
{
    /// <summary>Total amount collected in date range</summary>
    public decimal TotalCollected { get; set; }

    /// <summary>Total amount pending (due but not yet collected)</summary>
    public decimal TotalPending { get; set; }

    /// <summary>Total amount overdue (past due date)</summary>
    public decimal TotalOverdue { get; set; }

    /// <summary>Total expected amount for students with fees</summary>
    public decimal TotalExpected { get; set; }

    /// <summary>Collection percentage (collected/expected * 100)</summary>
    public decimal CollectionRate { get; set; }

    /// <summary>Number of students with paid fees</summary>
    public int PaidStudents { get; set; }

    /// <summary>Number of students with partial fees</summary>
    public int PartialStudents { get; set; }

    /// <summary>Number of students with due fees</summary>
    public int DueStudents { get; set; }

    /// <summary>Number of students with overdue fees</summary>
    public int OverdueStudents { get; set; }

    /// <summary>Comparison with previous period's collection rate</summary>
    public decimal? PreviousPeriodCollectionRate { get; set; }

    /// <summary>Percentage change from previous period</summary>
    public decimal? CollectionRateTrend { get; set; }
}

/// <summary>
/// Monthly collection trend data
/// </summary>
public class MonthlyCollectionTrendDto
{
    /// <summary>Year-Month format: "2026-01"</summary>
    public string Month { get; set; } = string.Empty;

    /// <summary>Amount collected in this month</summary>
    public decimal Collected { get; set; }

    /// <summary>Amount pending in this month</summary>
    public decimal Pending { get; set; }

    /// <summary>Amount overdue in this month</summary>
    public decimal Overdue { get; set; }

    /// <summary>Collection rate for this month</summary>
    public decimal CollectionRate { get; set; }

    /// <summary>Total expected for this month</summary>
    public decimal Expected { get; set; }
}

/// <summary>
/// Fee collection by category breakdown
/// </summary>
public class FeeCollectionByCategoryDto
{
    /// <summary>Category name (e.g., "Tuition", "Transport")</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>Amount collected for this category</summary>
    public decimal Collected { get; set; }

    /// <summary>Amount pending for this category</summary>
    public decimal Pending { get; set; }

    /// <summary>Amount overdue for this category</summary>
    public decimal Overdue { get; set; }

    /// <summary>Total expected amount for this category</summary>
    public decimal Expected { get; set; }

    /// <summary>Collection percentage for this category</summary>
    public decimal CollectionPercentage { get; set; }

    /// <summary>Percentage of total collections (for pie chart)</summary>
    public decimal PercentageOfTotal { get; set; }

    /// <summary>Number of records in this category</summary>
    public int Count { get; set; }
}

/// <summary>
/// Outstanding fees analysis (aging report)
/// </summary>
public class OutstandingFeeDto
{
    /// <summary>Student ID</summary>
    public string StudentId { get; set; } = string.Empty;

    /// <summary>Student name and enrollment number</summary>
    public string StudentInfo { get; set; } = string.Empty; // "Rahul (ENR001)"

    /// <summary>Class and section</summary>
    public string ClassSection { get; set; } = string.Empty; // "Class 9 - A"

    /// <summary>Total amount due</summary>
    public decimal DueAmount { get; set; }

    /// <summary>Number of days overdue</summary>
    public int DaysOverdue { get; set; }

    /// <summary>Original due date</summary>
    public DateTime DueDate { get; set; }

    /// <summary>Date of last payment</summary>
    public DateTime? LastPaymentDate { get; set; }

    /// <summary>Aging bucket: "0-30", "31-60", "61-90", "90+"</summary>
    public string AgingBucket { get; set; } = string.Empty;

    /// <summary>Reason for non-payment</summary>
    public string? Remarks { get; set; }

    /// <summary>Parent/Guardian contact info</summary>
    public string? ContactInfo { get; set; }

    /// <summary>Student status: Active/Inactive</summary>
    public bool IsActive { get; set; }
}

/// <summary>
/// Student payment history over time
/// </summary>
public class StudentPaymentHistoryDto
{
    /// <summary>Month of fee (YYYY-MM format)</summary>
    public string Month { get; set; } = string.Empty;

    /// <summary>Due amount for this month</summary>
    public decimal DueAmount { get; set; }

    /// <summary>Amount paid in this month</summary>
    public decimal PaidAmount { get; set; }

    /// <summary>Payment status: Paid, Partial, Due, Overdue</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Payment method used</summary>
    public string? PaymentMethod { get; set; }

    /// <summary>Date when fee was due</summary>
    public DateTime DueDate { get; set; }

    /// <summary>Date of payment (null if not paid)</summary>
    public DateTime? PaymentDate { get; set; }

    /// <summary>Reference/Receipt number</summary>
    public string? ReferenceNumber { get; set; }

    /// <summary>Remaining balance</summary>
    public decimal Balance { get; set; }
}

/// <summary>
/// Salary expense summary
/// </summary>
public class SalaryExpenseSummaryDto
{
    /// <summary>Total net salary paid</summary>
    public decimal TotalNetSalary { get; set; }

    /// <summary>Average salary per teacher</summary>
    public decimal AverageSalary { get; set; }

    /// <summary>Total base salary</summary>
    public decimal TotalBaseSalary { get; set; }

    /// <summary>Total bonus paid</summary>
    public decimal TotalBonus { get; set; }

    /// <summary>Total deductions</summary>
    public decimal TotalDeductions { get; set; }

    /// <summary>Number of staff paid</summary>
    public int StaffCount { get; set; }

    /// <summary>Number of staff who got bonus</summary>
    public int BonusRecipients { get; set; }

    /// <summary>Bonus as percentage of base salary</summary>
    public decimal BonusPercentage { get; set; }

    /// <summary>Deduction as percentage of base salary</summary>
    public decimal DeductionPercentage { get; set; }

    /// <summary>Comparison with previous period's total</summary>
    public decimal? PreviousPeriodTotal { get; set; }

    /// <summary>Percentage change from previous period</summary>
    public decimal? ExpenseTrend { get; set; }
}

/// <summary>
/// Monthly salary expense trend
/// </summary>
public class MonthlySalaryTrendDto
{
    /// <summary>Year-Month format: "2026-01"</summary>
    public string Month { get; set; } = string.Empty;

    /// <summary>Total net salary for this month</summary>
    public decimal TotalNetSalary { get; set; }

    /// <summary>Total base salary</summary>
    public decimal TotalBaseSalary { get; set; }

    /// <summary>Total bonus paid</summary>
    public decimal TotalBonus { get; set; }

    /// <summary>Total deductions</summary>
    public decimal TotalDeductions { get; set; }

    /// <summary>Number of staff</summary>
    public int StaffCount { get; set; }

    /// <summary>Staff who received bonus</summary>
    public int BonusRecipients { get; set; }

    /// <summary>Average salary for the month</summary>
    public decimal AverageSalary { get; set; }
}

/// <summary>
/// Salary component breakdown
/// </summary>
public class SalaryComponentBreakdownDto
{
    /// <summary>Total base salary component</summary>
    public decimal BaseSalary { get; set; }

    /// <summary>Total bonus component</summary>
    public decimal Bonus { get; set; }

    /// <summary>Total deductions component</summary>
    public decimal Deductions { get; set; }

    /// <summary>Total net salary (Base + Bonus - Deductions)</summary>
    public decimal NetSalary { get; set; }

    /// <summary>Base as percentage of net</summary>
    public decimal BasePercentage { get; set; }

    /// <summary>Bonus as percentage of net</summary>
    public decimal BonusPercentage { get; set; }

    /// <summary>Deductions as percentage of net</summary>
    public decimal DeductionsPercentage { get; set; }

    /// <summary>Number of salary records included</summary>
    public int RecordCount { get; set; }
}

/// <summary>
/// Teacher-wise salary comparison
/// </summary>
public class StaffSalaryComparisonDto
{
    /// <summary>Staff ID</summary>
    public string StaffId { get; set; } = string.Empty;

    /// <summary>Staff full name</summary>
    public string StaffName { get; set; } = string.Empty;

    /// <summary>Base salary</summary>
    public decimal BaseSalary { get; set; }

    /// <summary>Bonus amount</summary>
    public decimal Bonus { get; set; }

    /// <summary>Total deductions</summary>
    public decimal Deductions { get; set; }

    /// <summary>Net salary (Base + Bonus - Deductions)</summary>
    public decimal NetSalary { get; set; }

    /// <summary>Attendance percentage (if available)</summary>
    public decimal? AttendancePercentage { get; set; }

    /// <summary>Bonus eligible: Yes/No</summary>
    public bool BonusEligible { get; set; }

    /// <summary>Payment status: Pending, Approved, Paid</summary>
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// Attendance to salary correlation view
/// </summary>
public class AttendanceToSalaryCorrelationDto
{
    /// <summary>Staff ID</summary>
    public string StaffId { get; set; } = string.Empty;

    /// <summary>Staff name</summary>
    public string StaffName { get; set; } = string.Empty;

    /// <summary>Attendance percentage</summary>
    public decimal AttendancePercentage { get; set; }

    /// <summary>Number of days present</summary>
    public int PresentDays { get; set; }

    /// <summary>Number of days absent</summary>
    public int AbsentDays { get; set; }

    /// <summary>Total working days</summary>
    public int TotalDays { get; set; }

    /// <summary>Deduction calculated from formula</summary>
    public decimal CalculatedDeduction { get; set; }

    /// <summary>Actual deduction applied</summary>
    public decimal ActualDeduction { get; set; }

    /// <summary>Difference between calculated and actual</summary>
    public decimal DeductionDifference { get; set; }

    /// <summary>Bonus eligible (attendance >= 90%)</summary>
    public bool BonusEligible { get; set; }

    /// <summary>Bonus amount if eligible</summary>
    public decimal BonusAmount { get; set; }

    /// <summary>Base salary</summary>
    public decimal BaseSalary { get; set; }

    /// <summary>Flag if deduction doesn't match policy</summary>
    public bool HasDiscrepancy { get; set; }

    /// <summary>Reason for discrepancy if exists</summary>
    public string? DiscrepancyReason { get; set; }
}

public class BudgetVsActualDto
{
    public decimal BudgetedAmount { get; set; }
    public decimal ActualAmount { get; set; }
    public decimal Variance { get; set; }
    public decimal VariancePercentage { get; set; }
    public string Month { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // "Fee Collection" or "Salary Expenses"
}
