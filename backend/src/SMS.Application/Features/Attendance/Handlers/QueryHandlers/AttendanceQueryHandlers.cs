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
    private readonly IAcademicYearContext _academicYearContext;

    public GetStudentAttendanceByIdQueryHandler(IApplicationDbContext context, IAcademicYearContext academicYearContext)
    {
        _context = context;
        _academicYearContext = academicYearContext;
    }

    public async Task<StudentAttendanceDto?> Handle(GetStudentAttendanceByIdQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var attendanceId))
            return null;

        var attendance = await _context.StudentAttendances
            .Include(a => a.Enrollment)
            .FirstOrDefaultAsync(a => a.Id == attendanceId, cancellationToken);

        if (attendance == null)
            return null;

        return new StudentAttendanceDto
        {
            Id = attendance.Id.ToString(),
            StudentId = attendance.Enrollment?.StudentId.ToString() ?? "",
            SectionId = attendance.Enrollment?.SectionId.ToString() ?? "",
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
    private readonly IAcademicYearContext _academicYearContext;

    public GetStudentAttendanceByDateQueryHandler(IApplicationDbContext context, IAcademicYearContext academicYearContext)
    {
        _context = context;
        _academicYearContext = academicYearContext;
    }

    public async Task<List<StudentAttendanceDto>> Handle(GetStudentAttendanceByDateQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.SectionId, out var sectionId))
            return new List<StudentAttendanceDto>();

        var attendanceDate = DateOnly.FromDateTime(request.AttendanceDate);

        var attendances = await _context.StudentAttendances
            .Include(a => a.Enrollment)
            .Where(a => a.Enrollment.SectionId == sectionId && a.AttendanceDate == attendanceDate && a.Enrollment.AcademicYearId == _academicYearContext.RequiredAcademicYearId)
            .OrderBy(a => a.Enrollment.StudentId)
            .ToListAsync(cancellationToken);

        return attendances.Select(a => new StudentAttendanceDto
        {
            Id = a.Id.ToString(),
            StudentId = a.Enrollment?.StudentId.ToString() ?? "",
            SectionId = a.Enrollment?.SectionId.ToString() ?? "",
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
    private readonly IAcademicYearContext _academicYearContext;

    public GetStudentAttendanceHistoryQueryHandler(IApplicationDbContext context, IAcademicYearContext academicYearContext)
    {
        _context = context;
        _academicYearContext = academicYearContext;
    }

    public async Task<PaginatedStudentAttendanceListDto> Handle(GetStudentAttendanceHistoryQuery request, CancellationToken cancellationToken)
    {
        var query = _context.StudentAttendances.Include(a => a.Enrollment)
            .Where(a => a.Enrollment.AcademicYearId == _academicYearContext.RequiredAcademicYearId)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(request.StudentId) && Guid.TryParse(request.StudentId, out var studentId))
            query = query.Where(a => a.Enrollment.StudentId == studentId);

        if (!string.IsNullOrEmpty(request.SectionId) && Guid.TryParse(request.SectionId, out var sectionId))
            query = query.Where(a => a.Enrollment.SectionId == sectionId);

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
                _context.Enrollments.Include(e => e.Student),
                attendance => attendance.EnrollmentId,
                enrollment => enrollment.Id,
                (attendance, enrollment) => new StudentAttendanceListDto
                {
                    Id = attendance.Id.ToString(),
                    StudentId = enrollment.StudentId.ToString(),
                    StudentEnrollmentNumber = enrollment.Student != null ? enrollment.Student.EnrollmentNumber : "",
                    StudentName = enrollment.Student != null ? enrollment.Student.FirstName + " " + enrollment.Student.LastName : "",
                    SectionId = enrollment.SectionId.ToString(),
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
    private readonly IAcademicYearContext _academicYearContext;

    public GetStudentAttendanceSummaryQueryHandler(IApplicationDbContext context, IAcademicYearContext academicYearContext)
    {
        _context = context;
        _academicYearContext = academicYearContext;
    }

    public async Task<AttendanceStatisticsDto> Handle(GetStudentAttendanceSummaryQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.StudentId, out var studentId))
            throw new InvalidOperationException($"Invalid student ID format: {request.StudentId}");

        var query = _context.StudentAttendances
            .Include(a => a.Enrollment)
            .Where(a => a.Enrollment.StudentId == studentId && a.Enrollment.AcademicYearId == _academicYearContext.RequiredAcademicYearId);

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
/// Handler for GetStaffAttendanceByIdQuery
/// </summary>
public class GetStaffAttendanceByIdQueryHandler : IRequestHandler<GetStaffAttendanceByIdQuery, StaffAttendanceDto?>
{
    private readonly IApplicationDbContext _context;

    public GetStaffAttendanceByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StaffAttendanceDto?> Handle(GetStaffAttendanceByIdQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var attendanceId))
            return null;

        var attendance = await _context.StaffAttendances
            .Include(a => a.Staff)
                .ThenInclude(s => s.UserProfile)
            .FirstOrDefaultAsync(a => a.Id == attendanceId, cancellationToken);

        if (attendance == null)
            return null;

        return new StaffAttendanceDto
        {
            Id = attendance.Id.ToString(),
            StaffId = attendance.StaffId.ToString(),
            StaffName = attendance.Staff?.FullName,
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
/// Handler for GetStaffAttendanceByDateQuery
/// </summary>
public class GetStaffAttendanceByDateQueryHandler : IRequestHandler<GetStaffAttendanceByDateQuery, List<StaffAttendanceDto>>
{
    private readonly IApplicationDbContext _context;

    public GetStaffAttendanceByDateQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<StaffAttendanceDto>> Handle(GetStaffAttendanceByDateQuery request, CancellationToken cancellationToken)
    {
        var attendanceDate = DateOnly.FromDateTime(request.AttendanceDate);

        var attendances = await _context.StaffAttendances
            .Include(a => a.Staff)
                .ThenInclude(s => s.UserProfile)
            .Where(a => a.AttendanceDate == attendanceDate)
            .OrderBy(a => a.Staff != null ? a.Staff.UserProfile.FirstName : string.Empty)
            .ToListAsync(cancellationToken);

        return attendances.Select(a => new StaffAttendanceDto
        {
            Id = a.Id.ToString(),
            StaffId = a.StaffId.ToString(),
            StaffName = a.Staff?.FullName,
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
/// Handler for GetStaffAttendanceHistoryQuery
/// </summary>
public class GetStaffAttendanceHistoryQueryHandler : IRequestHandler<GetStaffAttendanceHistoryQuery, PaginatedStaffAttendanceListDto>
{
    private readonly IApplicationDbContext _context;

    public GetStaffAttendanceHistoryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedStaffAttendanceListDto> Handle(GetStaffAttendanceHistoryQuery request, CancellationToken cancellationToken)
    {
        var query = _context.StaffAttendances.AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(request.StaffId) && Guid.TryParse(request.StaffId, out var staffId))
            query = query.Where(a => a.StaffId == staffId);

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
            .Include(a => a.Staff)
                .ThenInclude(s => s.UserProfile)
            .OrderByDescending(a => a.AttendanceDate)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(a => new StaffAttendanceListDto
            {
                Id = a.Id.ToString(),
                StaffId = a.StaffId.ToString(),
                StaffName = a.Staff != null ? a.Staff.FullName : string.Empty,
                StaffEmail = (a.Staff != null && a.Staff.UserProfile != null) ? a.Staff.UserProfile.Email : string.Empty,
                AttendanceDate = a.AttendanceDate.ToDateTime(TimeOnly.MinValue),
                Status = a.Status
            })
            .ToListAsync(cancellationToken);

        return new PaginatedStaffAttendanceListDto
        {
            Items = attendances,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}

/// <summary>
/// Handler for GetStaffAttendanceSummaryQuery
/// </summary>
public class GetStaffAttendanceSummaryQueryHandler : IRequestHandler<GetStaffAttendanceSummaryQuery, AttendanceStatisticsDto>
{
    private readonly IApplicationDbContext _context;

    public GetStaffAttendanceSummaryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AttendanceStatisticsDto> Handle(GetStaffAttendanceSummaryQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.StaffId, out var staffId))
            throw new InvalidOperationException($"Invalid staff ID format: {request.StaffId}");

        var query = _context.StaffAttendances
            .Where(a => a.StaffId == staffId);

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
    private readonly IAcademicYearContext _academicYearContext;

    public GetMonthlyAttendanceReportQueryHandler(IApplicationDbContext context, IAcademicYearContext academicYearContext)
    {
        _context = context;
        _academicYearContext = academicYearContext;
    }

    public async Task<PaginatedMonthlyAttendanceReportDto> Handle(GetMonthlyAttendanceReportQuery request, CancellationToken cancellationToken)
    {
        var startDate = new DateOnly(request.Year, request.Month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var studentAttendances = await _context.StudentAttendances
            .Where(a => a.AttendanceDate >= startDate && a.AttendanceDate <= endDate && a.Enrollment.AcademicYearId == _academicYearContext.RequiredAcademicYearId)
            .Join(
                _context.Enrollments.Include(e => e.Student),
                attendance => attendance.EnrollmentId,
                enrollment => enrollment.Id,
                (attendance, enrollment) => new { attendance, enrollment })
            .ToListAsync(cancellationToken);

        var sections = await _context.Sections
            .Select(s => new { s.Id, s.SectionName }).ToListAsync(cancellationToken);

        var sectionMap = sections.ToDictionary(s => s.Id.ToString(), s => s.SectionName);
        var reportItems = new List<MonthlyAttendanceReportDto>();

        var studentGroups = studentAttendances.GroupBy(g => new { g.enrollment.StudentId, g.enrollment.SectionId });

        foreach (var group in studentGroups)
        {
            var student = group.First().enrollment.Student;
            var presentDays = group.Count(a => a.attendance.Status.ToLower() == "present");
            var totalDays = group.Count();
            var percentage = totalDays > 0 ? Math.Round((decimal)presentDays / totalDays * 100, 2) : 0;
            var status = percentage >= 75 ? "Good" : percentage >= 50 ? "Warning" : "Critical";
            var sectionName = sectionMap.TryGetValue(group.Key.SectionId.ToString(), out var name) ? name : "Unknown";

            reportItems.Add(new MonthlyAttendanceReportDto
            {
                StudentId = group.Key.StudentId.ToString(),
                StudentName = student != null ? student.FirstName + " " + student.LastName : "",
                EnrollmentNumber = student != null ? student.EnrollmentNumber : "",
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
    private readonly IAcademicYearContext _academicYearContext;

    public GetLowAttendanceAlertsQueryHandler(IApplicationDbContext context, IAcademicYearContext academicYearContext)
    {
        _context = context;
        _academicYearContext = academicYearContext;
    }

    public async Task<List<LowAttendanceAlertDto>> Handle(GetLowAttendanceAlertsQuery request, CancellationToken cancellationToken)
    {
        var startDate = new DateOnly(request.Year, request.Month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var attendances = await _context.StudentAttendances
            .Where(a => a.AttendanceDate >= startDate && a.AttendanceDate <= endDate && a.Enrollment.AcademicYearId == _academicYearContext.RequiredAcademicYearId)
            .Join(
                _context.Enrollments.Include(e => e.Student),
                attendance => attendance.EnrollmentId,
                enrollment => enrollment.Id,
                (attendance, enrollment) => new { attendance, enrollment })
            .ToListAsync(cancellationToken);

        var sections = await _context.Sections
            .Select(s => new { s.Id, s.SectionName }).ToListAsync(cancellationToken);

        var sectionMap = sections.ToDictionary(s => s.Id.ToString(), s => s.SectionName);
        var alerts = new List<LowAttendanceAlertDto>();

        foreach (var group in attendances.GroupBy(a => new { a.enrollment.StudentId, a.enrollment.SectionId }))
        {
            var absentCount = group.Count(a => a.attendance.Status.ToLower() == "absent");
            var totalDays = group.Count();
            var percentage = totalDays > 0 ? Math.Round((decimal)(totalDays - absentCount) / totalDays * 100, 2) : 0;

            if (percentage < request.AttendanceThreshold)
            {
                var student = group.First().enrollment.Student;
                var lastAbsent = group.Where(a => a.attendance.Status.ToLower() == "absent")
                    .OrderByDescending(a => a.attendance.AttendanceDate).FirstOrDefault();
                var sectionName = sectionMap.TryGetValue(group.Key.SectionId.ToString(), out var name) ? name : "Unknown";

                alerts.Add(new LowAttendanceAlertDto
                {
                    StudentId = group.Key.StudentId.ToString(),
                    StudentName = student != null ? student.FirstName + " " + student.LastName : "",
                    EnrollmentNumber = student != null ? student.EnrollmentNumber : "",
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
    private readonly IAcademicYearContext _academicYearContext;

    public GetClassAttendanceSummaryQueryHandler(IApplicationDbContext context, IAcademicYearContext academicYearContext)
    {
        _context = context;
        _academicYearContext = academicYearContext;
    }

    public async Task<List<ClassAttendanceSummaryDto>> Handle(GetClassAttendanceSummaryQuery request, CancellationToken cancellationToken)
    {
        var startDate = new DateOnly(request.Year, request.Month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var studentAttendances = await _context.StudentAttendances
            .Where(a => a.AttendanceDate >= startDate && a.AttendanceDate <= endDate && a.Enrollment.AcademicYearId == _academicYearContext.RequiredAcademicYearId)
            .Join(
                _context.Enrollments.Include(e => e.Student),
                attendance => attendance.EnrollmentId,
                enrollment => enrollment.Id,
                (attendance, enrollment) => new { attendance, enrollment })
            .ToListAsync(cancellationToken);

        var sections = await _context.Sections.Include(s => s.Class).ToListAsync(cancellationToken);
        var results = new List<ClassAttendanceSummaryDto>();

        foreach (var section in sections)
        {
            var secAttendances = studentAttendances.Where(a => a.enrollment.SectionId == section.Id).ToList();
            if (!secAttendances.Any()) continue;

            var percentages = new List<decimal>();
            foreach (var studentGroup in secAttendances.GroupBy(a => a.enrollment.StudentId))
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
