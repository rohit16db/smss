using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Attendance.Commands;
using SMS.Application.Features.Attendance.DTOs;
using SMS.Domain.Entities;

namespace SMS.Application.Features.Attendance.Handlers.CommandHandlers;

/// <summary>
/// Handler for MarkStudentAttendanceCommand
/// </summary>
public class MarkStudentAttendanceCommandHandler : IRequestHandler<MarkStudentAttendanceCommand, StudentAttendanceDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IAcademicYearContext _academicYearContext;

    public MarkStudentAttendanceCommandHandler(IApplicationDbContext context, IAcademicYearContext academicYearContext)
    {
        _context = context;
        _academicYearContext = academicYearContext;
    }

    public async Task<StudentAttendanceDto> Handle(MarkStudentAttendanceCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.StudentId, out var studentId))
            throw new InvalidOperationException($"Invalid student ID format: {request.StudentId}");

        if (!Guid.TryParse(request.CreatedByUserId, out var markedByUserId))
            throw new InvalidOperationException($"Invalid user ID format: {request.CreatedByUserId}");

        // Auto-detect section from student's active enrollment for the current academic year
        var enrollment = await _context.Enrollments
            .Where(ss => ss.StudentId == studentId && ss.Status == "Enrolled" && ss.AcademicYearId == _academicYearContext.RequiredAcademicYearId)
            .FirstOrDefaultAsync(cancellationToken);

        if (enrollment == null)
            throw new InvalidOperationException($"Student {request.StudentId} is not enrolled in any active section. Please enroll the student in a section before marking attendance.");

        var attendanceDate = DateOnly.FromDateTime(request.AttendanceDate);

        // Check if attendance already exists for this student on this date
        var existingAttendance = await _context.StudentAttendances
            .FirstOrDefaultAsync(a => a.EnrollmentId == enrollment.Id && 
                                     a.AttendanceDate == attendanceDate, 
                                cancellationToken);

        if (existingAttendance != null)
            throw new InvalidOperationException($"Attendance already marked for student {request.StudentId} on {request.AttendanceDate:dd/MM/yyyy}");

        var attendance = new StudentAttendance
        {
            Id = Guid.NewGuid(),
            EnrollmentId = enrollment.Id,
            AttendanceDate = attendanceDate,
            Status = request.Status.ToLower(),
            Reason = request.Reason,
            MarkedByUserId = markedByUserId,
            MarkedAt = DateTime.UtcNow,
            CreatedBy = request.CreatedByUserId,
            UpdatedBy = request.CreatedByUserId
        };

        _context.StudentAttendances.Add(attendance);
        await _context.SaveChangesAsync(cancellationToken);

        return new StudentAttendanceDto
        {
            Id = attendance.Id.ToString(),
            StudentId = enrollment.StudentId.ToString(),
            SectionId = enrollment.SectionId.ToString(),
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
/// Handler for UpdateStudentAttendanceCommand
/// </summary>
public class UpdateStudentAttendanceCommandHandler : IRequestHandler<UpdateStudentAttendanceCommand, StudentAttendanceDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IAcademicYearContext _academicYearContext;

    public UpdateStudentAttendanceCommandHandler(IApplicationDbContext context, IAcademicYearContext academicYearContext)
    {
        _context = context;
        _academicYearContext = academicYearContext;
    }

    public async Task<StudentAttendanceDto> Handle(UpdateStudentAttendanceCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var attendanceId))
            throw new InvalidOperationException($"Invalid attendance ID format: {request.Id}");

        var attendance = await _context.StudentAttendances
            .Include(a => a.Enrollment)
            .FirstOrDefaultAsync(a => a.Id == attendanceId && a.Enrollment.AcademicYearId == _academicYearContext.RequiredAcademicYearId, cancellationToken)
            ?? throw new InvalidOperationException($"Student attendance with ID {request.Id} not found");

        attendance.Status = request.Status.ToLower();
        attendance.Reason = request.Reason;
        attendance.UpdatedBy = request.UpdatedByUserId;

        _context.StudentAttendances.Update(attendance);
        await _context.SaveChangesAsync(cancellationToken);

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
/// Handler for DeleteStudentAttendanceCommand
/// </summary>
public class DeleteStudentAttendanceCommandHandler : IRequestHandler<DeleteStudentAttendanceCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IAcademicYearContext _academicYearContext;

    public DeleteStudentAttendanceCommandHandler(IApplicationDbContext context, IAcademicYearContext academicYearContext)
    {
        _context = context;
        _academicYearContext = academicYearContext;
    }

    public async Task<bool> Handle(DeleteStudentAttendanceCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var attendanceId))
            throw new InvalidOperationException($"Invalid attendance ID format: {request.Id}");

        var attendance = await _context.StudentAttendances
            .Include(a => a.Enrollment)
            .FirstOrDefaultAsync(a => a.Id == attendanceId && a.Enrollment.AcademicYearId == _academicYearContext.RequiredAcademicYearId, cancellationToken)
            ?? throw new InvalidOperationException($"Student attendance with ID {request.Id} not found");

        _context.StudentAttendances.Remove(attendance);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}

/// <summary>
/// Handler for RecordStaffAttendanceCommand
/// </summary>
public class RecordStaffAttendanceCommandHandler : IRequestHandler<RecordStaffAttendanceCommand, StaffAttendanceDto>
{
    private readonly IApplicationDbContext _context;

    public RecordStaffAttendanceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StaffAttendanceDto> Handle(RecordStaffAttendanceCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.StaffId, out var staffId))
            throw new InvalidOperationException($"Invalid staff ID format: {request.StaffId}");

        if (!Guid.TryParse(request.CreatedByUserId, out var recordedByUserId))
            throw new InvalidOperationException($"Invalid user ID format: {request.CreatedByUserId}");

        var attendanceDate = DateOnly.FromDateTime(request.AttendanceDate);

        // Check if attendance already exists for this staff on this date
        var existingAttendance = await _context.StaffAttendances
            .FirstOrDefaultAsync(a => a.StaffId == staffId && 
                                     a.AttendanceDate == attendanceDate, 
                                 cancellationToken);

        if (existingAttendance != null)
            throw new InvalidOperationException($"Attendance already recorded for staff {request.StaffId} on {request.AttendanceDate:dd/MM/yyyy}");

        var staff = await _context.Staff
            .Include(s => s.UserProfile)
            .FirstOrDefaultAsync(t => t.Id == staffId, cancellationToken);

        var attendance = new StaffAttendance
        {
            Id = Guid.NewGuid(),
            StaffId = staffId,
            AttendanceDate = attendanceDate,
            Status = request.Status.ToLower(),
            Reason = request.Reason,
            RecordedByUserId = recordedByUserId,
            RecordedAt = DateTime.UtcNow,
            CreatedBy = request.CreatedByUserId,
            UpdatedBy = request.CreatedByUserId
        };

        _context.StaffAttendances.Add(attendance);
        await _context.SaveChangesAsync(cancellationToken);

        return new StaffAttendanceDto
        {
            Id = attendance.Id.ToString(),
            StaffId = attendance.StaffId.ToString(),
            StaffName = staff?.FullName,
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
/// Handler for UpdateStaffAttendanceCommand
/// </summary>
public class UpdateStaffAttendanceCommandHandler : IRequestHandler<UpdateStaffAttendanceCommand, StaffAttendanceDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateStaffAttendanceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StaffAttendanceDto> Handle(UpdateStaffAttendanceCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var attendanceId))
            throw new InvalidOperationException($"Invalid attendance ID format: {request.Id}");

        var attendance = await _context.StaffAttendances
            .Include(a => a.Staff)
                .ThenInclude(s => s.UserProfile)
            .FirstOrDefaultAsync(a => a.Id == attendanceId, cancellationToken)
            ?? throw new InvalidOperationException($"Staff attendance with ID {request.Id} not found");

        attendance.Status = request.Status.ToLower();
        attendance.Reason = request.Reason;
        attendance.UpdatedBy = request.UpdatedByUserId;

        _context.StaffAttendances.Update(attendance);
        await _context.SaveChangesAsync(cancellationToken);

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
/// Handler for DeleteStaffAttendanceCommand
/// </summary>
public class DeleteStaffAttendanceCommandHandler : IRequestHandler<DeleteStaffAttendanceCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteStaffAttendanceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteStaffAttendanceCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var attendanceId))
            throw new InvalidOperationException($"Invalid attendance ID format: {request.Id}");

        var attendance = await _context.StaffAttendances
            .FirstOrDefaultAsync(a => a.Id == attendanceId, cancellationToken)
            ?? throw new InvalidOperationException($"Teacher attendance with ID {request.Id} not found");

        _context.StaffAttendances.Remove(attendance);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
