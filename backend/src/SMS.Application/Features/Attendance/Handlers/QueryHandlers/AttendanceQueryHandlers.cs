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
