using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.StaffManagement.Commands;
using SMS.Application.Features.StaffManagement.DTOs;
using SMS.Application.Features.StaffManagement.Queries;
using SMS.Domain.Entities;
using SMS.Domain.Enums;

namespace SMS.Application.Features.StaffManagement.Handlers;

/// <summary>
/// Handler for creating teacher assignments
/// </summary>
public class CreateStaffAssignmentHandler : IRequestHandler<CreateStaffAssignmentCommand, StaffAssignmentDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IAcademicYearContext _academicYearContext;

    public CreateStaffAssignmentHandler(IApplicationDbContext context, IAcademicYearContext academicYearContext)
    {
        _context = context;
        _academicYearContext = academicYearContext;
    }

    public async Task<StaffAssignmentDto> Handle(CreateStaffAssignmentCommand request, CancellationToken cancellationToken)
    {
        // Validate staff member exists and is a teacher
        var staff = await _context.Staff
            .FirstOrDefaultAsync(s => s.Id == request.StaffId, cancellationToken);
        
        if (staff == null)
            throw new KeyNotFoundException($"Staff with ID {request.StaffId} not found");

        if (staff.RoleType != UserRole.Teacher)
            throw new InvalidOperationException("Only staff with the 'Teacher' role can be assigned to academic classes.");

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

        // Check for duplicate active assignment in the current academic year
        var duplicateExists = await _context.StaffAssignments
            .AnyAsync(ta => ta.StaffId == request.StaffId 
                && ta.ClassId == request.ClassId 
                && ta.SubjectId == request.SubjectId 
                && ta.AcademicYearId == _academicYearContext.RequiredAcademicYearId
                && ta.RemovalDate == null, cancellationToken);

        if (duplicateExists)
            throw new InvalidOperationException(
                $"Teacher is already assigned to this class and subject combination in the current session");

        var assignment = new StaffAssignment
        {
            Id = Guid.NewGuid(),
            StaffId = request.StaffId,
            ClassId = request.ClassId,
            SubjectId = request.SubjectId,
            AcademicYearId = _academicYearContext.RequiredAcademicYearId,
            AssignmentDate = request.AssignmentDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.StaffAssignments.Add(assignment);
        await _context.SaveChangesAsync(cancellationToken);

        return new StaffAssignmentDto
        {
            Id = assignment.Id,
            StaffId = assignment.StaffId,
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
public class RemoveStaffAssignmentHandler : IRequestHandler<RemoveStaffAssignmentCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public RemoveStaffAssignmentHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(RemoveStaffAssignmentCommand request, CancellationToken cancellationToken)
    {
        var assignment = await _context.StaffAssignments
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
public class GetStaffAssignmentsHandler : IRequestHandler<GetStaffAssignmentsQuery, List<StaffAssignmentDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAcademicYearContext _academicYearContext;

    public GetStaffAssignmentsHandler(IApplicationDbContext context, IAcademicYearContext academicYearContext)
    {
        _context = context;
        _academicYearContext = academicYearContext;
    }

    public async Task<List<StaffAssignmentDto>> Handle(GetStaffAssignmentsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.StaffAssignments
            .Include(ta => ta.Staff)
            .Where(ta => ta.StaffId == request.StaffId && ta.AcademicYearId == _academicYearContext.RequiredAcademicYearId);

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

        return assignments.Select(a => new StaffAssignmentDto
        {
            Id = a.Id,
            StaffId = a.StaffId,
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
