using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Teachers.Commands;
using SMS.Application.Features.Teachers.DTOs;
using SMS.Application.Features.Teachers.Queries;
using SMS.Domain.Entities;

namespace SMS.Application.Features.Teachers.Handlers;

/// <summary>
/// Handler for creating teacher assignments
/// </summary>
public class CreateTeacherAssignmentHandler : IRequestHandler<CreateTeacherAssignmentCommand, TeacherAssignmentDto>
{
    private readonly IApplicationDbContext _context;

    public CreateTeacherAssignmentHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TeacherAssignmentDto> Handle(CreateTeacherAssignmentCommand request, CancellationToken cancellationToken)
    {
        // Validate teacher exists
        var teacherExists = await _context.Teachers
            .AnyAsync(t => t.Id == request.TeacherId, cancellationToken);
        if (!teacherExists)
            throw new KeyNotFoundException($"Teacher with ID {request.TeacherId} not found");

        // Validate class exists
        var classEntity = await _context.Classes
            .FirstOrDefaultAsync(c => c.Id == request.ClassId, cancellationToken);
        if (classEntity == null)
            throw new KeyNotFoundException($"Class with ID {request.ClassId} not found");

        // Validate subject exists
        var subject = await _context.Subjects
            .FirstOrDefaultAsync(s => s.Id == request.SubjectId, cancellationToken);
        if (subject == null)
            throw new KeyNotFoundException($"Subject with ID {request.SubjectId} not found");

        // Check for duplicate active assignment
        var duplicateExists = await _context.TeacherAssignments
            .AnyAsync(ta => ta.TeacherId == request.TeacherId 
                && ta.ClassId == request.ClassId 
                && ta.SubjectId == request.SubjectId 
                && ta.RemovalDate == null, cancellationToken);

        if (duplicateExists)
            throw new InvalidOperationException(
                $"Teacher is already assigned to this class and subject combination");

        var assignment = new TeacherAssignment
        {
            Id = Guid.NewGuid(),
            TeacherId = request.TeacherId,
            ClassId = request.ClassId,
            SubjectId = request.SubjectId,
            AssignmentDate = request.AssignmentDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.TeacherAssignments.Add(assignment);
        await _context.SaveChangesAsync(cancellationToken);

        return new TeacherAssignmentDto
        {
            Id = assignment.Id,
            TeacherId = assignment.TeacherId,
            ClassId = assignment.ClassId,
            SubjectId = assignment.SubjectId,
            AssignmentDate = assignment.AssignmentDate,
            RemovalDate = assignment.RemovalDate,
            ClassName = classEntity.Name,
            SubjectName = subject.Name,
            SubjectCode = subject.Code,
            IsActive = assignment.RemovalDate == null
        };
    }
}

/// <summary>
/// Handler for removing teacher assignments
/// </summary>
public class RemoveTeacherAssignmentHandler : IRequestHandler<RemoveTeacherAssignmentCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public RemoveTeacherAssignmentHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(RemoveTeacherAssignmentCommand request, CancellationToken cancellationToken)
    {
        var assignment = await _context.TeacherAssignments
            .FirstOrDefaultAsync(ta => ta.Id == request.AssignmentId, cancellationToken);

        if (assignment == null)
            throw new KeyNotFoundException($"Assignment with ID {request.AssignmentId} not found");

        if (assignment.RemovalDate != null)
            throw new InvalidOperationException("Assignment has already been removed");

        assignment.RemovalDate = request.RemovalDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        assignment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

/// <summary>
/// Handler for getting teacher assignments
/// </summary>
public class GetTeacherAssignmentsHandler : IRequestHandler<GetTeacherAssignmentsQuery, List<TeacherAssignmentDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTeacherAssignmentsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TeacherAssignmentDto>> Handle(GetTeacherAssignmentsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.TeacherAssignments
            .Include(ta => ta.Teacher)
            .Where(ta => ta.TeacherId == request.TeacherId);

        if (request.ActiveOnly == true)
        {
            query = query.Where(ta => ta.RemovalDate == null);
        }

        var assignments = await query
            .OrderByDescending(ta => ta.AssignmentDate)
            .ToListAsync(cancellationToken);

        // Get related entities
        var classIds = assignments.Select(a => a.ClassId).Distinct().ToList();
        var subjectIds = assignments.Select(a => a.SubjectId).Distinct().ToList();

        var classes = await _context.Classes
            .Where(c => classIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);

        var subjects = await _context.Subjects
            .Where(s => subjectIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, cancellationToken);

        return assignments.Select(a => new TeacherAssignmentDto
        {
            Id = a.Id,
            TeacherId = a.TeacherId,
            ClassId = a.ClassId,
            SubjectId = a.SubjectId,
            AssignmentDate = a.AssignmentDate,
            RemovalDate = a.RemovalDate,
            ClassName = classes.GetValueOrDefault(a.ClassId),
            SubjectName = subjects.GetValueOrDefault(a.SubjectId)?.Name,
            SubjectCode = subjects.GetValueOrDefault(a.SubjectId)?.Code,
            IsActive = a.RemovalDate == null
        }).ToList();
    }
}
