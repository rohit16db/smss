using MediatR;
using SMS.Application.Features.Attendance.DTOs;

namespace SMS.Application.Features.Attendance.Commands;

/// <summary>
/// Command to mark student attendance
/// Section is auto-detected from student's current enrollment
/// </summary>
public class MarkStudentAttendanceCommand : IRequest<StudentAttendanceDto>
{
    public string StudentId { get; set; } = string.Empty;
    // SectionId removed - auto-detected from student_sections
    public DateTime AttendanceDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
}

/// <summary>
/// Command to update student attendance
/// </summary>
public class UpdateStudentAttendanceCommand : IRequest<StudentAttendanceDto>
{
    public string Id { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string UpdatedByUserId { get; set; } = string.Empty;
}

/// <summary>
/// Command to delete student attendance record
/// </summary>
public class DeleteStudentAttendanceCommand : IRequest<bool>
{
    public string Id { get; set; } = string.Empty;
}

/// <summary>
/// Command to record staff attendance
/// </summary>
public class RecordStaffAttendanceCommand : IRequest<StaffAttendanceDto>
{
    public string StaffId { get; set; } = string.Empty;
    public DateTime AttendanceDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
}

/// <summary>
/// Command to update staff attendance
/// </summary>
public class UpdateStaffAttendanceCommand : IRequest<StaffAttendanceDto>
{
    public string Id { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string UpdatedByUserId { get; set; } = string.Empty;
}

/// <summary>
/// Command to delete staff attendance record
/// </summary>
public class DeleteStaffAttendanceCommand : IRequest<bool>
{
    public string Id { get; set; } = string.Empty;
}
