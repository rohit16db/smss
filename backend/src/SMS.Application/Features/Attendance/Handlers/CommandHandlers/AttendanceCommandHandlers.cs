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

    public MarkStudentAttendanceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StudentAttendanceDto> Handle(MarkStudentAttendanceCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.StudentId, out var studentId))
            throw new InvalidOperationException($"Invalid student ID format: {request.StudentId}");

        if (!Guid.TryParse(request.CreatedByUserId, out var markedByUserId))
            throw new InvalidOperationException($"Invalid user ID format: {request.CreatedByUserId}");

        // Auto-detect section from student's current enrollment
        var currentSection = await _context.StudentSections
            .Where(ss => ss.StudentId == studentId && ss.IsCurrent == true)
            .Select(ss => ss.SectionId)
            .FirstOrDefaultAsync(cancellationToken);

        if (currentSection == Guid.Empty || currentSection == default)
            throw new InvalidOperationException($"Student {request.StudentId} is not enrolled in any active section. Please enroll the student in a section before marking attendance.");

        var attendanceDate = DateOnly.FromDateTime(request.AttendanceDate);

        // Check if attendance already exists for this student on this date
        var existingAttendance = await _context.StudentAttendances
            .FirstOrDefaultAsync(a => a.StudentId == studentId && 
                                     a.SectionId == currentSection && 
                                     a.AttendanceDate == attendanceDate, 
                                cancellationToken);

        if (existingAttendance != null)
            throw new InvalidOperationException($"Attendance already marked for student {request.StudentId} on {request.AttendanceDate:dd/MM/yyyy}");

        var attendance = new StudentAttendance
        {
            Id = Guid.NewGuid(),
            StudentId = studentId,
            SectionId = currentSection,  // Auto-detected from enrollment
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
/// Handler for UpdateStudentAttendanceCommand
/// </summary>
public class UpdateStudentAttendanceCommandHandler : IRequestHandler<UpdateStudentAttendanceCommand, StudentAttendanceDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateStudentAttendanceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StudentAttendanceDto> Handle(UpdateStudentAttendanceCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var attendanceId))
            throw new InvalidOperationException($"Invalid attendance ID format: {request.Id}");

        var attendance = await _context.StudentAttendances
            .FirstOrDefaultAsync(a => a.Id == attendanceId, cancellationToken)
            ?? throw new InvalidOperationException($"Student attendance with ID {request.Id} not found");

        attendance.Status = request.Status.ToLower();
        attendance.Reason = request.Reason;
        attendance.UpdatedBy = request.UpdatedByUserId;

        _context.StudentAttendances.Update(attendance);
        await _context.SaveChangesAsync(cancellationToken);

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
/// Handler for DeleteStudentAttendanceCommand
/// </summary>
public class DeleteStudentAttendanceCommandHandler : IRequestHandler<DeleteStudentAttendanceCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteStudentAttendanceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteStudentAttendanceCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var attendanceId))
            throw new InvalidOperationException($"Invalid attendance ID format: {request.Id}");

        var attendance = await _context.StudentAttendances
            .FirstOrDefaultAsync(a => a.Id == attendanceId, cancellationToken)
            ?? throw new InvalidOperationException($"Student attendance with ID {request.Id} not found");

        _context.StudentAttendances.Remove(attendance);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}

/// <summary>
/// Handler for RecordTeacherAttendanceCommand
/// </summary>
public class RecordTeacherAttendanceCommandHandler : IRequestHandler<RecordTeacherAttendanceCommand, TeacherAttendanceDto>
{
    private readonly IApplicationDbContext _context;

    public RecordTeacherAttendanceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TeacherAttendanceDto> Handle(RecordTeacherAttendanceCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.TeacherId, out var teacherId))
            throw new InvalidOperationException($"Invalid teacher ID format: {request.TeacherId}");

        if (!Guid.TryParse(request.CreatedByUserId, out var recordedByUserId))
            throw new InvalidOperationException($"Invalid user ID format: {request.CreatedByUserId}");

        var attendanceDate = DateOnly.FromDateTime(request.AttendanceDate);

        // Check if attendance already exists for this teacher on this date
        var existingAttendance = await _context.TeacherAttendances
            .FirstOrDefaultAsync(a => a.TeacherId == teacherId && 
                                     a.AttendanceDate == attendanceDate, 
                                cancellationToken);

        if (existingAttendance != null)
            throw new InvalidOperationException($"Attendance already recorded for teacher {request.TeacherId} on {request.AttendanceDate:dd/MM/yyyy}");

        var teacher = await _context.Teachers
            .FirstOrDefaultAsync(t => t.Id == teacherId, cancellationToken);

        var attendance = new TeacherAttendance
        {
            Id = Guid.NewGuid(),
            TeacherId = teacherId,
            AttendanceDate = attendanceDate,
            Status = request.Status.ToLower(),
            Reason = request.Reason,
            RecordedByUserId = recordedByUserId,
            RecordedAt = DateTime.UtcNow,
            CreatedBy = request.CreatedByUserId,
            UpdatedBy = request.CreatedByUserId
        };

        _context.TeacherAttendances.Add(attendance);
        await _context.SaveChangesAsync(cancellationToken);

        return new TeacherAttendanceDto
        {
            Id = attendance.Id.ToString(),
            TeacherId = attendance.TeacherId.ToString(),
            TeacherName = teacher != null ? $"{teacher.FirstName} {teacher.LastName}" : null,
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
/// Handler for UpdateTeacherAttendanceCommand
/// </summary>
public class UpdateTeacherAttendanceCommandHandler : IRequestHandler<UpdateTeacherAttendanceCommand, TeacherAttendanceDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateTeacherAttendanceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TeacherAttendanceDto> Handle(UpdateTeacherAttendanceCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var attendanceId))
            throw new InvalidOperationException($"Invalid attendance ID format: {request.Id}");

        var attendance = await _context.TeacherAttendances
            .Include(a => a.Teacher)
            .FirstOrDefaultAsync(a => a.Id == attendanceId, cancellationToken)
            ?? throw new InvalidOperationException($"Teacher attendance with ID {request.Id} not found");

        attendance.Status = request.Status.ToLower();
        attendance.Reason = request.Reason;
        attendance.UpdatedBy = request.UpdatedByUserId;

        _context.TeacherAttendances.Update(attendance);
        await _context.SaveChangesAsync(cancellationToken);

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
/// Handler for DeleteTeacherAttendanceCommand
/// </summary>
public class DeleteTeacherAttendanceCommandHandler : IRequestHandler<DeleteTeacherAttendanceCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteTeacherAttendanceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteTeacherAttendanceCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var attendanceId))
            throw new InvalidOperationException($"Invalid attendance ID format: {request.Id}");

        var attendance = await _context.TeacherAttendances
            .FirstOrDefaultAsync(a => a.Id == attendanceId, cancellationToken)
            ?? throw new InvalidOperationException($"Teacher attendance with ID {request.Id} not found");

        _context.TeacherAttendances.Remove(attendance);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
