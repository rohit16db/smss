using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Transport.Commands;
using SMS.Domain.Entities;

namespace SMS.Application.Features.Transport.Handlers;

public class AssignStudentToTransportCommandHandler : IRequestHandler<AssignStudentToTransportCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public AssignStudentToTransportCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(AssignStudentToTransportCommand request, CancellationToken cancellationToken)
    {
        // 1. Validation - Try direct enrollment lookup first
        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.Id == request.EnrollmentId, cancellationToken);

        // If not found, check if the ID is a StudentId and find their active enrollment
        if (enrollment == null)
        {
            enrollment = await _context.Enrollments
                .Where(e => e.StudentId == request.EnrollmentId && e.Status == "Enrolled")
                .OrderByDescending(e => e.EnrollmentDate)
                .FirstOrDefaultAsync(cancellationToken);
        }

        if (enrollment == null) throw new KeyNotFoundException("Active enrollment for the student not found.");

        var route = await _context.TransportRoutes
            .FirstOrDefaultAsync(r => r.Id == request.RouteId, cancellationToken);

        if (route == null) throw new KeyNotFoundException("Transport route not found.");

        // 2. Deactivate existing active assignment for this enrollment
        var activeAssignments = await _context.StudentTransportAssignments
            .Where(a => a.EnrollmentId == enrollment.Id && a.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var assignment in activeAssignments)
        {
            assignment.IsActive = false;
            assignment.UpdatedAt = DateTime.UtcNow;
        }

        // 3. Create new assignment
        var newAssignment = new StudentTransportAssignment
        {
            Id = Guid.NewGuid(),
            EnrollmentId = enrollment.Id,
            RouteId = request.RouteId,
            RouteStopId = request.RouteStopId,
            EffectiveDate = request.EffectiveDate.ToUniversalTime(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.StudentTransportAssignments.Add(newAssignment);

        // 4. Update Student Fee
        var studentFee = await _context.StudentFees
            .FirstOrDefaultAsync(sf => sf.EnrollmentId == enrollment.Id && sf.IsActive, cancellationToken);

        if (studentFee != null)
        {
            studentFee.TransportFeeAmount = route.MonthlyFee;
            studentFee.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return newAssignment.Id;
    }
}
