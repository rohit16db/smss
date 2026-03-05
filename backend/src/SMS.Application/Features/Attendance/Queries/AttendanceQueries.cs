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
/// Query to get student attendance by date and section
/// </summary>
public class GetStudentAttendanceByDateQuery : IRequest<List<StudentAttendanceDto>>
{
    public string SectionId { get; set; } = string.Empty;
    public DateTime AttendanceDate { get; set; }
}

/// <summary>
/// Query to get student attendance history with pagination
/// </summary>
public class GetStudentAttendanceHistoryQuery : IRequest<PaginatedStudentAttendanceListDto>
{
    public string? StudentId { get; set; }
    public string? SectionId { get; set; }
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
/// <summary>
/// Query to get monthly attendance report for students
/// </summary>
public class GetMonthlyAttendanceReportQuery : IRequest<PaginatedMonthlyAttendanceReportDto>
{
    public string? StudentId { get; set; }
    public string? SectionId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal? MinAttendancePercentage { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

/// <summary>
/// Query to get low attendance alerts (students with <75% attendance)
/// </summary>
public class GetLowAttendanceAlertsQuery : IRequest<List<LowAttendanceAlertDto>>
{
    public string? SectionId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal AttendanceThreshold { get; set; } = 75m; // Default 75%
}

/// <summary>
/// Query to get class attendance summary for a specific month
/// </summary>
public class GetClassAttendanceSummaryQuery : IRequest<List<ClassAttendanceSummaryDto>>
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string? SectionId { get; set; }
}