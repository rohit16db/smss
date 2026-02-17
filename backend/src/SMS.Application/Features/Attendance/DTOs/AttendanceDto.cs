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
/// DTO for recording teacher attendance
/// </summary>
public class RecordTeacherAttendanceDto
{
    public string TeacherId { get; set; } = string.Empty;
    public DateTime AttendanceDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
}

/// <summary>
/// DTO for updating teacher attendance record
/// </summary>
public class UpdateTeacherAttendanceDto
{
    public string Id { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
}

/// <summary>
/// DTO for teacher attendance details
/// </summary>
public class TeacherAttendanceDto
{
    public string Id { get; set; } = string.Empty;
    public string TeacherId { get; set; } = string.Empty;
    public string? TeacherName { get; set; }
    public DateTime AttendanceDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string? RecordedByUserId { get; set; }
    public DateTime RecordedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for teacher attendance list items
/// </summary>
public class TeacherAttendanceListDto
{
    public string Id { get; set; } = string.Empty;
    public string TeacherId { get; set; } = string.Empty;
    public string? TeacherName { get; set; }
    public string? TeacherEmail { get; set; }
    public DateTime AttendanceDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// DTO for paginated teacher attendance list
/// </summary>
public class PaginatedTeacherAttendanceListDto
{
    public List<TeacherAttendanceListDto> Items { get; set; } = new();
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
    public int TotalLeave { get; set; }
    public int TotalUnexcused { get; set; }
    public int TotalRecords { get; set; }
    public decimal AttendancePercentage { get; set; }
}
