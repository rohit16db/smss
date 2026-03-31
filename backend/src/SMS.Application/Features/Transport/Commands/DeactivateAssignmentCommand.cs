using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;

namespace SMS.Application.Features.Transport.Commands;

public class DeactivateAssignmentCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}

public class DeactivateAssignmentHandler : IRequestHandler<DeactivateAssignmentCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeactivateAssignmentHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeactivateAssignmentCommand request, CancellationToken cancellationToken)
    {
        var assignment = await _context.StudentTransportAssignments
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (assignment == null) return false;

        // 1. Mark assignment as inactive
        assignment.IsActive = false;
        assignment.UpdatedAt = DateTime.UtcNow;

        // 2. Find the student's active fee record and reset transport fee to 0
        var studentFee = await _context.StudentFees
            .FirstOrDefaultAsync(sf => sf.EnrollmentId == assignment.EnrollmentId && sf.IsActive, cancellationToken);

        if (studentFee != null)
        {
            studentFee.TransportFeeAmount = 0;
            studentFee.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
