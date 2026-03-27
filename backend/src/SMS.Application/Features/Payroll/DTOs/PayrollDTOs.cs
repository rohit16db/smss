namespace SMS.Application.Features.Payroll.DTOs;

/// <summary>
/// Staff attendance record for payroll
/// </summary>
public class StaffAttendancePayrollDto
{
    public Guid Id { get; set; }
    public Guid StaffId { get; set; }
    public string StaffName { get; set; } = string.Empty;
    public DateOnly AttendanceDate { get; set; }
    public string Status { get; set; } = string.Empty; // Present, Absent, Leave, etc.
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Staff payroll report for a specific period
/// </summary>
public class StaffPayrollReportDto
{
    public Guid StaffId { get; set; }
    public string StaffName { get; set; } = string.Empty;
    public string? StaffImagePath { get; set; }
    public decimal BaseSalary { get; set; }
    public DateOnly PeriodStartDate { get; set; }
    public DateOnly PeriodEndDate { get; set; }
    
    // Attendance metrics
    public int TotalWorkingDays { get; set; }
    public int PresentDays { get; set; }
    public int AbsentDays { get; set; }
    public int LeaveDays { get; set; }
    public decimal AttendancePercentage { get; set; }
    
    // Payroll calculations
    public decimal GrossSalary { get; set; }
    public decimal DeductionsForAbsence { get; set; }
    public decimal BonusAmount { get; set; }
    public decimal NetSalary { get; set; }
    
    // Bonus eligibility
    public bool IsBonusEligible { get; set; }
    public string BonusEligibilityReason { get; set; } = string.Empty;
}

/// <summary>
/// Bonus eligibility calculation
/// </summary>
public class BonusEligibilityDto
{
    public Guid StaffId { get; set; }
    public string StaffName { get; set; } = string.Empty;
    public decimal AttendancePercentage { get; set; }
    public decimal BonusPercentage { get; set; }
    public decimal BonusAmount { get; set; }
    public bool IsEligible { get; set; }
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Summary of staff attendance for a period
/// </summary>
public class StaffAttendanceSummaryDto
{
    public Guid StaffId { get; set; }
    public string StaffName { get; set; } = string.Empty;
    public int TotalDays { get; set; }
    public int PresentDays { get; set; }
    public int AbsentDays { get; set; }
    public int LeaveDays { get; set; }
    public decimal AttendancePercentage { get; set; }
}

/// <summary>
/// Payroll period report
/// </summary>
public class PayrollPeriodReportDto
{
    public DateTime GeneratedAt { get; set; }
    public DateOnly PeriodStartDate { get; set; }
    public DateOnly PeriodEndDate { get; set; }
    public List<StaffPayrollReportDto> StaffPayrolls { get; set; } = new();
    public decimal TotalPayrollAmount { get; set; }
    public decimal TotalBonusAmount { get; set; }
    public int EligibleStaffs { get; set; }
}
