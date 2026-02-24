namespace SMS.Application.Features.Payroll.DTOs;

/// <summary>
/// Teacher attendance record for payroll
/// </summary>
public class TeacherAttendancePayrollDto
{
    public Guid Id { get; set; }
    public Guid TeacherId { get; set; }
    public string TeacherName { get; set; }
    public DateOnly AttendanceDate { get; set; }
    public string Status { get; set; } // Present, Absent, Leave, etc.
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Teacher payroll report for a specific period
/// </summary>
public class TeacherPayrollReportDto
{
    public Guid TeacherId { get; set; }
    public string TeacherName { get; set; }
    public string? TeacherImagePath { get; set; }
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
    public string BonusEligibilityReason { get; set; }
}

/// <summary>
/// Bonus eligibility calculation
/// </summary>
public class BonusEligibilityDto
{
    public Guid TeacherId { get; set; }
    public string TeacherName { get; set; }
    public decimal AttendancePercentage { get; set; }
    public decimal BonusPercentage { get; set; }
    public decimal BonusAmount { get; set; }
    public bool IsEligible { get; set; }
    public string Reason { get; set; }
}

/// <summary>
/// Summary of teacher attendance for a period
/// </summary>
public class TeacherAttendanceSummaryDto
{
    public Guid TeacherId { get; set; }
    public string TeacherName { get; set; }
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
    public List<TeacherPayrollReportDto> TeacherPayrolls { get; set; }
    public decimal TotalPayrollAmount { get; set; }
    public decimal TotalBonusAmount { get; set; }
    public int EligibleTeachers { get; set; }
}
