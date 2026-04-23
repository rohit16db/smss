namespace SMS.Application.Features.Attendance.DTOs;

/// <summary>
/// DTO for creating student attendance record
/// Section is auto-detected from student's current enrollment
/// </summary>
public class MarkStudentAttendanceDto
{
    public string StudentId { get; set; } = string.Empty;
    // SectionId removed - auto-detected from student_sections where is_current = true
    public DateTime AttendanceDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
}

/// <summary>
/// DTO for updating student attendance record
/// </summary>
public class UpdateStudentAttendanceDto
{
    public string Id { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
}

/// <summary>
/// DTO for student attendance details
/// </summary>
public class StudentAttendanceDto
{
    public string Id { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public string SectionId { get; set; } = string.Empty;
    public DateTime AttendanceDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string? MarkedByUserId { get; set; }
    public DateTime MarkedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for student attendance list items
/// </summary>
public class StudentAttendanceListDto
{
    public string Id { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public string StudentEnrollmentNumber { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string SectionId { get; set; } = string.Empty;
    public DateTime AttendanceDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string? GuardianPhone { get; set; }
}

/// <summary>
/// DTO for paginated student attendance list
/// </summary>
public class PaginatedStudentAttendanceListDto
{
    public List<StudentAttendanceListDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

/// <summary>
/// DTO for recording staff attendance
/// </summary>
public class RecordStaffAttendanceDto
{
    public string StaffId { get; set; } = string.Empty;
    public DateTime AttendanceDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
}

/// <summary>
/// DTO for updating staff attendance record
/// </summary>
public class UpdateStaffAttendanceDto
{
    public string Id { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
}

/// <summary>
/// DTO for staff attendance details
/// </summary>
public class StaffAttendanceDto
{
    public string Id { get; set; } = string.Empty;
    public string StaffId { get; set; } = string.Empty;
    public string? StaffName { get; set; }
    public DateTime AttendanceDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string? RecordedByUserId { get; set; }
    public DateTime RecordedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for staff attendance list items
/// </summary>
public class StaffAttendanceListDto
{
    public string Id { get; set; } = string.Empty;
    public string StaffId { get; set; } = string.Empty;
    public string? StaffName { get; set; }
    public string? StaffEmail { get; set; }
    public DateTime AttendanceDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// DTO for paginated staff attendance list
/// </summary>
public class PaginatedStaffAttendanceListDto
{
    public List<StaffAttendanceListDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

/// <summary>
/// DTO for attendance summary statistics
/// </summary>
public class AttendanceStatisticsDto
{
    public int TotalPresent { get; set; }
    public int TotalAbsent { get; set; }
    public int TotalLate { get; set; }
    public int TotalLeave { get; set; }
    public int TotalUnexcused { get; set; }
    public int TotalRecords { get; set; }
    public decimal AttendancePercentage { get; set; }
}

/// <summary>
/// DTO for monthly attendance report item
/// </summary>
public class MonthlyAttendanceReportDto
{
    public string StudentId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string EnrollmentNumber { get; set; } = string.Empty;
    public string SectionId { get; set; } = string.Empty;
    public string SectionName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public int TotalWorkingDays { get; set; }
    public int PresentDays { get; set; }
    public int AbsentDays { get; set; }
    public int LateDays { get; set; }
    public int LeaveDays { get; set; }
    public decimal AttendancePercentage { get; set; }
    public string AttendanceStatus { get; set; } = string.Empty; // "Good" (>=75%), "Warning" (50-75%), "Critical" (<50%)
}

/// <summary>
/// DTO for paginated monthly attendance report
/// </summary>
public class PaginatedMonthlyAttendanceReportDto
{
    public List<MonthlyAttendanceReportDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public decimal AverageAttendancePercentage { get; set; }
    public int LowAttendanceCount { get; set; } // Count with <75%
}

/// <summary>
/// DTO for low attendance alert
/// </summary>
public class LowAttendanceAlertDto
{
    public string StudentId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string EnrollmentNumber { get; set; } = string.Empty;
    public string SectionId { get; set; } = string.Empty;
    public string SectionName { get; set; } = string.Empty;
    public decimal AttendancePercentage { get; set; }
    public int AbsentDays { get; set; }
    public int TotalDays { get; set; }
    public string AlertLevel { get; set; } = string.Empty; // "Warning" (50-75%), "Critical" (<50%)
    public DateTime LastAbsentDate { get; set; }
}

/// <summary>
/// DTO for class attendance summary
/// </summary>
public class ClassAttendanceSummaryDto
{
    public string SectionId { get; set; } = string.Empty;
    public string SectionName { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public int TotalStudents { get; set; }
    public decimal AverageAttendancePercentage { get; set; }
    public int HighAttendanceCount { get; set; } // >= 75%
    public int MediumAttendanceCount { get; set; } // 50-75%
    public int LowAttendanceCount { get; set; } // < 50%
    public int Year { get; set; }
    public int Month { get; set; }
}

/// <summary>
/// Result DTO for bulk attendance operation
/// </summary>
public class BulkAttendanceResultDto
{
    public int TotalProcessed { get; set; }
    public int Created { get; set; }
    public int Updated { get; set; }
    public int Failed { get; set; }
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// DTO for bulk marking student attendance (API request body)
/// </summary>
public class BulkMarkStudentAttendanceDto
{
    public string SectionId { get; set; } = string.Empty;
    public DateTime AttendanceDate { get; set; }
    public List<BulkAttendanceEntryDto> Entries { get; set; } = new();
}

/// <summary>
/// Individual entry within a bulk attendance request
/// </summary>
public class BulkAttendanceEntryDto
{
    public string StudentId { get; set; } = string.Empty;
    public string Status { get; set; } = "present";
    public string? Reason { get; set; }
}

