using MediatR;
using SMS.Application.Features.Attendance.DTOs;

namespace SMS.Application.Features.Attendance.Queries;

/// <summary>
/// Query to get student attendance by ID
/// </summary>
public class GetStudentAttendanceByIdQuery : IRequest<StudentAttendanceDto?>
{
    public string Id { get; set; } = string.Empty;
}

/// <summary>
/// Query to get student attendance by date and class
/// </summary>
public class GetStudentAttendanceByDateQuery : IRequest<List<StudentAttendanceDto>>
{
    public string ClassId { get; set; } = string.Empty;
    public DateTime AttendanceDate { get; set; }
}

/// <summary>
/// Query to get student attendance history with pagination
/// </summary>
public class GetStudentAttendanceHistoryQuery : IRequest<PaginatedStudentAttendanceListDto>
{
    public string? StudentId { get; set; }
    public string? ClassId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Status { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

/// <summary>
/// Query to get student attendance summary
/// </summary>
public class GetStudentAttendanceSummaryQuery : IRequest<AttendanceStatisticsDto>
{
    public string StudentId { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

/// <summary>
/// Query to get teacher attendance by ID
/// </summary>
public class GetTeacherAttendanceByIdQuery : IRequest<TeacherAttendanceDto?>
{
    public string Id { get; set; } = string.Empty;
}

/// <summary>
/// Query to get teacher attendance by date
/// </summary>
public class GetTeacherAttendanceByDateQuery : IRequest<List<TeacherAttendanceDto>>
{
    public DateTime AttendanceDate { get; set; }
}

/// <summary>
/// Query to get teacher attendance history with pagination
/// </summary>
public class GetTeacherAttendanceHistoryQuery : IRequest<PaginatedTeacherAttendanceListDto>
{
    public string? TeacherId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Status { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

/// <summary>
/// Query to get teacher attendance summary
/// </summary>
public class GetTeacherAttendanceSummaryQuery : IRequest<AttendanceStatisticsDto>
{
    public string TeacherId { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
