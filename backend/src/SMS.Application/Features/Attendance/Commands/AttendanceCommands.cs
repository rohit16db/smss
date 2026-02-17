using MediatR;
using SMS.Application.Features.Attendance.DTOs;

namespace SMS.Application.Features.Attendance.Commands;

/// <summary>
/// Command to mark student attendance
/// </summary>
public class MarkStudentAttendanceCommand : IRequest<StudentAttendanceDto>
{
    public string StudentId { get; set; } = string.Empty;
    public string ClassId { get; set; } = string.Empty;
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
/// Command to record teacher attendance
/// </summary>
public class RecordTeacherAttendanceCommand : IRequest<TeacherAttendanceDto>
{
    public string TeacherId { get; set; } = string.Empty;
    public DateTime AttendanceDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
}

/// <summary>
/// Command to update teacher attendance
/// </summary>
public class UpdateTeacherAttendanceCommand : IRequest<TeacherAttendanceDto>
{
    public string Id { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string UpdatedByUserId { get; set; } = string.Empty;
}

/// <summary>
/// Command to delete teacher attendance record
/// </summary>
public class DeleteTeacherAttendanceCommand : IRequest<bool>
{
    public string Id { get; set; } = string.Empty;
}
