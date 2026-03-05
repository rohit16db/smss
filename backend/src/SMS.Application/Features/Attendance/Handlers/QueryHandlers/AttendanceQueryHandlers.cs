using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Attendance.DTOs;
using SMS.Application.Features.Attendance.Queries;
using SMS.Domain.Enums;

namespace SMS.Application.Features.Attendance.Handlers.QueryHandlers;

/// <summary>
/// Handler for GetStudentAttendanceByIdQuery
/// </summary>
public class GetStudentAttendanceByIdQueryHandler : IRequestHandler<GetStudentAttendanceByIdQuery, StudentAttendanceDto?>
{
    private readonly IApplicationDbContext _context;

    public GetStudentAttendanceByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StudentAttendanceDto?> Handle(GetStudentAttendanceByIdQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var attendanceId))
            return null;

        var attendance = await _context.StudentAttendances
            .FirstOrDefaultAsync(a => a.Id == attendanceId, cancellationToken);

        if (attendance == null)
            return null;

        return new StudentAttendanceDto
        {
            Id = attendance.Id.ToString(),
            StudentId = attendance.StudentId.ToString(),
            SectionId = attendance.SectionId.ToString(),
            AttendanceDate = attendance.AttendanceDate.ToDateTime(TimeOnly.MinValue),
            Status = attendance.Status,
            Reason = attendance.Reason,
            MarkedByUserId = attendance.MarkedByUserId?.ToString(),
            MarkedAt = attendance.MarkedAt,
            CreatedAt = attendance.CreatedAt
        };
    }
}

/// <summary>
/// Handler for GetStudentAttendanceByDateQuery
/// </summary>
public class GetStudentAttendanceByDateQueryHandler : IRequestHandler<GetStudentAttendanceByDateQuery, List<StudentAttendanceDto>>
{
    private readonly IApplicationDbContext _context;

    public GetStudentAttendanceByDateQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<StudentAttendanceDto>> Handle(GetStudentAttendanceByDateQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.SectionId, out var sectionId))
            return new List<StudentAttendanceDto>();

        var attendanceDate = DateOnly.FromDateTime(request.AttendanceDate);

        var attendances = await _context.StudentAttendances
            .Where(a => a.SectionId == sectionId && a.AttendanceDate == attendanceDate)
            .OrderBy(a => a.StudentId)
            .ToListAsync(cancellationToken);

        return attendances.Select(a => new StudentAttendanceDto
        {
            Id = a.Id.ToString(),
            StudentId = a.StudentId.ToString(),
            SectionId = a.SectionId.ToString(),
            AttendanceDate = a.AttendanceDate.ToDateTime(TimeOnly.MinValue),
            Status = a.Status,
            Reason = a.Reason,
            MarkedByUserId = a.MarkedByUserId?.ToString(),
            MarkedAt = a.MarkedAt,
            CreatedAt = a.CreatedAt
        }).ToList();
    }
}

/// <summary>
/// Handler for GetStudentAttendanceHistoryQuery
/// </summary>
public class GetStudentAttendanceHistoryQueryHandler : IRequestHandler<GetStudentAttendanceHistoryQuery, PaginatedStudentAttendanceListDto>
{
    private readonly IApplicationDbContext _context;

    public GetStudentAttendanceHistoryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedStudentAttendanceListDto> Handle(GetStudentAttendanceHistoryQuery request, CancellationToken cancellationToken)
    {
        var query = _context.StudentAttendances.AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(request.StudentId) && Guid.TryParse(request.StudentId, out var studentId))
            query = query.Where(a => a.StudentId == studentId);

        if (!string.IsNullOrEmpty(request.SectionId) && Guid.TryParse(request.SectionId, out var sectionId))
            query = query.Where(a => a.SectionId == sectionId);

        if (request.StartDate.HasValue)
        {
            var startDate = DateOnly.FromDateTime(request.StartDate.Value);
            query = query.Where(a => a.AttendanceDate >= startDate);
        }

        if (request.EndDate.HasValue)
        {
            var endDate = DateOnly.FromDateTime(request.EndDate.Value);
            query = query.Where(a => a.AttendanceDate <= endDate);
        }

        if (!string.IsNullOrEmpty(request.Status))
            query = query.Where(a => a.Status == request.Status.ToLower());

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination and include student details
        var attendances = await query
            .OrderByDescending(a => a.AttendanceDate)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Join(
                _context.Students,
                attendance => attendance.StudentId,
                student => student.Id,
                (attendance, student) => new StudentAttendanceListDto
                {
                    Id = attendance.Id.ToString(),
                    StudentId = attendance.StudentId.ToString(),
                    StudentEnrollmentNumber = student.EnrollmentNumber,
                    StudentName = student.FirstName + " " + student.LastName,
                    SectionId = attendance.SectionId.ToString(),
                    AttendanceDate = attendance.AttendanceDate.ToDateTime(TimeOnly.MinValue),
                    Status = attendance.Status,
                    Reason = attendance.Reason
                })
            .ToListAsync(cancellationToken);

        return new PaginatedStudentAttendanceListDto
        {
            Items = attendances,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}

/// <summary>
/// Handler for GetStudentAttendanceSummaryQuery
/// </summary>
public class GetStudentAttendanceSummaryQueryHandler : IRequestHandler<GetStudentAttendanceSummaryQuery, AttendanceStatisticsDto>
{
    private readonly IApplicationDbContext _context;

    public GetStudentAttendanceSummaryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AttendanceStatisticsDto> Handle(GetStudentAttendanceSummaryQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.StudentId, out var studentId))
            throw new InvalidOperationException($"Invalid student ID format: {request.StudentId}");

        var query = _context.StudentAttendances
            .Where(a => a.StudentId == studentId);

        if (request.StartDate.HasValue)
        {
            var startDate = DateOnly.FromDateTime(request.StartDate.Value);
            query = query.Where(a => a.AttendanceDate >= startDate);
        }

        if (request.EndDate.HasValue)
        {
            var endDate = DateOnly.FromDateTime(request.EndDate.Value);
            query = query.Where(a => a.AttendanceDate <= endDate);
        }

        var attendances = await query.ToListAsync(cancellationToken);

        var summary = new AttendanceStatisticsDto
        {
            TotalPresent = attendances.Count(a => a.Status == AttendanceStatus.Present),
            TotalAbsent = attendances.Count(a => a.Status == AttendanceStatus.Absent),
            TotalLate = attendances.Count(a => a.Status == AttendanceStatus.Late),
            TotalLeave = attendances.Count(a => a.Status == AttendanceStatus.Leave),
            TotalUnexcused = attendances.Count(a => a.Status == AttendanceStatus.Unexcused),
            TotalRecords = attendances.Count
        };

        summary.AttendancePercentage = summary.TotalRecords > 0
            ? Math.Round((decimal)summary.TotalPresent / summary.TotalRecords * 100, 2)
            : 0;

        return summary;
    }
}

/// <summary>
/// Handler for GetTeacherAttendanceByIdQuery
/// </summary>
public class GetTeacherAttendanceByIdQueryHandler : IRequestHandler<GetTeacherAttendanceByIdQuery, TeacherAttendanceDto?>
{
    private readonly IApplicationDbContext _context;

    public GetTeacherAttendanceByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TeacherAttendanceDto?> Handle(GetTeacherAttendanceByIdQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var attendanceId))
            return null;

        var attendance = await _context.TeacherAttendances
            .Include(a => a.Teacher)
            .FirstOrDefaultAsync(a => a.Id == attendanceId, cancellationToken);

        if (attendance == null)
            return null;

        return new TeacherAttendanceDto
        {
            Id = attendance.Id.ToString(),
            TeacherId = attendance.TeacherId.ToString(),
            TeacherName = attendance.Teacher != null ? $"{attendance.Teacher.FirstName} {attendance.Teacher.LastName}" : null,
            AttendanceDate = attendance.AttendanceDate.ToDateTime(TimeOnly.MinValue),
            Status = attendance.Status,
            Reason = attendance.Reason,
            RecordedByUserId = attendance.RecordedByUserId?.ToString(),
            RecordedAt = attendance.RecordedAt,
            CreatedAt = attendance.CreatedAt
        };
    }
}

/// <summary>
/// Handler for GetTeacherAttendanceByDateQuery
/// </summary>
public class GetTeacherAttendanceByDateQueryHandler : IRequestHandler<GetTeacherAttendanceByDateQuery, List<TeacherAttendanceDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTeacherAttendanceByDateQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TeacherAttendanceDto>> Handle(GetTeacherAttendanceByDateQuery request, CancellationToken cancellationToken)
    {
        var attendanceDate = DateOnly.FromDateTime(request.AttendanceDate);

        var attendances = await _context.TeacherAttendances
            .Include(a => a.Teacher)
            .Where(a => a.AttendanceDate == attendanceDate)
            .OrderBy(a => a.Teacher != null ? a.Teacher.FirstName : string.Empty)
            .ToListAsync(cancellationToken);

        return attendances.Select(a => new TeacherAttendanceDto
        {
            Id = a.Id.ToString(),
            TeacherId = a.TeacherId.ToString(),
            TeacherName = a.Teacher != null ? $"{a.Teacher.FirstName} {a.Teacher.LastName}" : null,
            AttendanceDate = a.AttendanceDate.ToDateTime(TimeOnly.MinValue),
            Status = a.Status,
            Reason = a.Reason,
            RecordedByUserId = a.RecordedByUserId?.ToString(),
            RecordedAt = a.RecordedAt,
            CreatedAt = a.CreatedAt
        }).ToList();
    }
}

/// <summary>
/// Handler for GetTeacherAttendanceHistoryQuery
/// </summary>
public class GetTeacherAttendanceHistoryQueryHandler : IRequestHandler<GetTeacherAttendanceHistoryQuery, PaginatedTeacherAttendanceListDto>
{
    private readonly IApplicationDbContext _context;

    public GetTeacherAttendanceHistoryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedTeacherAttendanceListDto> Handle(GetTeacherAttendanceHistoryQuery request, CancellationToken cancellationToken)
    {
        var query = _context.TeacherAttendances.AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(request.TeacherId) && Guid.TryParse(request.TeacherId, out var teacherId))
            query = query.Where(a => a.TeacherId == teacherId);

        if (request.StartDate.HasValue)
        {
            var startDate = DateOnly.FromDateTime(request.StartDate.Value);
            query = query.Where(a => a.AttendanceDate >= startDate);
        }

        if (request.EndDate.HasValue)
        {
            var endDate = DateOnly.FromDateTime(request.EndDate.Value);
            query = query.Where(a => a.AttendanceDate <= endDate);
        }

        if (!string.IsNullOrEmpty(request.Status))
            query = query.Where(a => a.Status == request.Status.ToLower());

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var attendances = await query
            .Include(a => a.Teacher)
            .OrderByDescending(a => a.AttendanceDate)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new TeacherAttendanceListDto
            {
                Id = a.Id.ToString(),
                TeacherId = a.TeacherId.ToString(),
                TeacherName = a.Teacher != null ? $"{a.Teacher.FirstName} {a.Teacher.LastName}" : null,
                TeacherEmail = a.Teacher != null ? a.Teacher.Email : null,
                AttendanceDate = a.AttendanceDate.ToDateTime(TimeOnly.MinValue),
                Status = a.Status
            })
            .ToListAsync(cancellationToken);

        return new PaginatedTeacherAttendanceListDto
        {
            Items = attendances,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}

/// <summary>
/// Handler for GetTeacherAttendanceSummaryQuery
/// </summary>
public class GetTeacherAttendanceSummaryQueryHandler : IRequestHandler<GetTeacherAttendanceSummaryQuery, AttendanceStatisticsDto>
{
    private readonly IApplicationDbContext _context;

    public GetTeacherAttendanceSummaryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AttendanceStatisticsDto> Handle(GetTeacherAttendanceSummaryQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.TeacherId, out var teacherId))
            throw new InvalidOperationException($"Invalid teacher ID format: {request.TeacherId}");

        var query = _context.TeacherAttendances
            .Where(a => a.TeacherId == teacherId);

        if (request.StartDate.HasValue)
        {
            var startDate = DateOnly.FromDateTime(request.StartDate.Value);
            query = query.Where(a => a.AttendanceDate >= startDate);
        }

        if (request.EndDate.HasValue)
        {
            var endDate = DateOnly.FromDateTime(request.EndDate.Value);
            query = query.Where(a => a.AttendanceDate <= endDate);
        }

        var attendances = await query.ToListAsync(cancellationToken);

        var summary = new AttendanceStatisticsDto
        {
            TotalPresent = attendances.Count(a => a.Status == AttendanceStatus.Present),
            TotalAbsent = attendances.Count(a => a.Status == AttendanceStatus.Absent),
            TotalLate = attendances.Count(a => a.Status == AttendanceStatus.Late),
            TotalLeave = attendances.Count(a => a.Status == AttendanceStatus.Leave),
            TotalUnexcused = attendances.Count(a => a.Status == AttendanceStatus.Unexcused),
            TotalRecords = attendances.Count
        };

        summary.AttendancePercentage = summary.TotalRecords > 0
            ? Math.Round((decimal)summary.TotalPresent / summary.TotalRecords * 100, 2)
            : 0;

        return summary;
    }
}
/// <summary>
/// Handler for GetMonthlyAttendanceReportQuery
/// </summary>
public class GetMonthlyAttendanceReportQueryHandler : IRequestHandler<GetMonthlyAttendanceReportQuery, PaginatedMonthlyAttendanceReportDto>
{
    private readonly IApplicationDbContext _context;

    public GetMonthlyAttendanceReportQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedMonthlyAttendanceReportDto> Handle(GetMonthlyAttendanceReportQuery request, CancellationToken cancellationToken)
    {
        var startDate = new DateOnly(request.Year, request.Month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var studentAttendances = await _context.StudentAttendances
            .Where(a => a.AttendanceDate >= startDate && a.AttendanceDate <= endDate)
            .Join(
                _context.Students,
                attendance => attendance.StudentId,
                student => student.Id,
                (attendance, student) => new { attendance, student })
            .ToListAsync(cancellationToken);

        var sections = await _context.Sections
            .Select(s => new { s.Id, s.SectionName }).ToListAsync(cancellationToken);

        var sectionMap = sections.ToDictionary(s => s.Id.ToString(), s => s.SectionName);
        var reportItems = new List<MonthlyAttendanceReportDto>();

        var studentGroups = studentAttendances.GroupBy(g => new { g.attendance.StudentId, g.attendance.SectionId });

        foreach (var group in studentGroups)
        {
            var student = group.First().student;
            var presentDays = group.Count(a => a.attendance.Status.ToLower() == "present");
            var totalDays = group.Count();
            var percentage = totalDays > 0 ? Math.Round((decimal)presentDays / totalDays * 100, 2) : 0;
            var status = percentage >= 75 ? "Good" : percentage >= 50 ? "Warning" : "Critical";
            var sectionName = sectionMap.TryGetValue(group.Key.SectionId.ToString(), out var name) ? name : "Unknown";

            reportItems.Add(new MonthlyAttendanceReportDto
            {
                StudentId = group.Key.StudentId.ToString(),
                StudentName = student.FirstName + " " + student.LastName,
                EnrollmentNumber = student.EnrollmentNumber,
                SectionId = group.Key.SectionId.ToString(),
                SectionName = sectionName,
                Year = request.Year,
                Month = request.Month,
                TotalWorkingDays = totalDays,
                PresentDays = presentDays,
                AbsentDays = group.Count(a => a.attendance.Status.ToLower() == "absent"),
                LateDays = group.Count(a => a.attendance.Status.ToLower() == "late"),
                LeaveDays = group.Count(a => a.attendance.Status.ToLower() == "leave"),
                AttendancePercentage = percentage,
                AttendanceStatus = status
            });
        }

        var totalCount = reportItems.Count;
        var average = reportItems.Any() ? Math.Round(reportItems.Average(r => r.AttendancePercentage), 2) : 0;
        var lowCount = reportItems.Count(r => r.AttendancePercentage < 75);

        var pagedItems = reportItems.OrderBy(r => r.StudentName)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize).ToList();

        return new PaginatedMonthlyAttendanceReportDto
        {
            Items = pagedItems,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            AverageAttendancePercentage = average,
            LowAttendanceCount = lowCount
        };
    }
}

/// <summary>
/// Handler for GetLowAttendanceAlertsQuery
/// </summary>
public class GetLowAttendanceAlertsQueryHandler : IRequestHandler<GetLowAttendanceAlertsQuery, List<LowAttendanceAlertDto>>
{
    private readonly IApplicationDbContext _context;

    public GetLowAttendanceAlertsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<LowAttendanceAlertDto>> Handle(GetLowAttendanceAlertsQuery request, CancellationToken cancellationToken)
    {
        var startDate = new DateOnly(request.Year, request.Month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var attendances = await _context.StudentAttendances
            .Where(a => a.AttendanceDate >= startDate && a.AttendanceDate <= endDate)
            .Join(
                _context.Students,
                attendance => attendance.StudentId,
                student => student.Id,
                (attendance, student) => new { attendance, student })
            .ToListAsync(cancellationToken);

        var sections = await _context.Sections
            .Select(s => new { s.Id, s.SectionName }).ToListAsync(cancellationToken);

        var sectionMap = sections.ToDictionary(s => s.Id.ToString(), s => s.SectionName);
        var alerts = new List<LowAttendanceAlertDto>();

        foreach (var group in attendances.GroupBy(a => new { a.attendance.StudentId, a.attendance.SectionId }))
        {
            var absentCount = group.Count(a => a.attendance.Status.ToLower() == "absent");
            var totalDays = group.Count();
            var percentage = totalDays > 0 ? Math.Round((decimal)(totalDays - absentCount) / totalDays * 100, 2) : 0;

            if (percentage < request.AttendanceThreshold)
            {
                var student = group.First().student;
                var lastAbsent = group.Where(a => a.attendance.Status.ToLower() == "absent")
                    .OrderByDescending(a => a.attendance.AttendanceDate).FirstOrDefault();
                var sectionName = sectionMap.TryGetValue(group.Key.SectionId.ToString(), out var name) ? name : "Unknown";

                alerts.Add(new LowAttendanceAlertDto
                {
                    StudentId = group.Key.StudentId.ToString(),
                    StudentName = student.FirstName + " " + student.LastName,
                    EnrollmentNumber = student.EnrollmentNumber,
                    SectionId = group.Key.SectionId.ToString(),
                    SectionName = sectionName,
                    AttendancePercentage = percentage,
                    AbsentDays = absentCount,
                    TotalDays = totalDays,
                    AlertLevel = percentage < 50 ? "Critical" : "Warning",
                    LastAbsentDate = lastAbsent?.attendance.AttendanceDate.ToDateTime(TimeOnly.MinValue) ?? DateTime.UtcNow
                });
            }
        }

        return alerts.OrderBy(a => a.AlertLevel).ThenBy(a => a.AttendancePercentage).ToList();
    }
}

/// <summary>
/// Handler for GetClassAttendanceSummaryQuery
/// </summary>
public class GetClassAttendanceSummaryQueryHandler : IRequestHandler<GetClassAttendanceSummaryQuery, List<ClassAttendanceSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetClassAttendanceSummaryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ClassAttendanceSummaryDto>> Handle(GetClassAttendanceSummaryQuery request, CancellationToken cancellationToken)
    {
        var startDate = new DateOnly(request.Year, request.Month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var studentAttendances = await _context.StudentAttendances
            .Where(a => a.AttendanceDate >= startDate && a.AttendanceDate <= endDate)
            .Join(
                _context.Students,
                attendance => attendance.StudentId,
                student => student.Id,
                (attendance, student) => new { attendance, student })
            .ToListAsync(cancellationToken);

        var sections = await _context.Sections.Include(s => s.Class).ToListAsync(cancellationToken);
        var results = new List<ClassAttendanceSummaryDto>();

        foreach (var section in sections)
        {
            var secAttendances = studentAttendances.Where(a => a.attendance.SectionId == section.Id).ToList();
            if (!secAttendances.Any()) continue;

            var percentages = new List<decimal>();
            foreach (var studentGroup in secAttendances.GroupBy(a => a.attendance.StudentId))
            {
                var present = studentGroup.Count(a => a.attendance.Status.ToLower() == "present");
                var total = studentGroup.Count();
                percentages.Add(total > 0 ? Math.Round((decimal)present / total * 100, 2) : 0);
            }

            results.Add(new ClassAttendanceSummaryDto
            {
                SectionId = section.Id.ToString(),
                SectionName = section.SectionName,
                ClassName = section.Class?.Name ?? "Unknown",
                TotalStudents = percentages.Count,
                AverageAttendancePercentage = percentages.Any() ? Math.Round(percentages.Average(), 2) : 0,
                HighAttendanceCount = percentages.Count(p => p >= 75),
                MediumAttendanceCount = percentages.Count(p => p >= 50 && p < 75),
                LowAttendanceCount = percentages.Count(p => p < 50),
                Year = request.Year,
                Month = request.Month
            });
        }

        return results.OrderBy(r => r.ClassName).ThenBy(r => r.SectionName).ToList();
    }
}
