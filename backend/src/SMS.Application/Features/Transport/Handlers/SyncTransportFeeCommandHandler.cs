using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Transport.Commands;

namespace SMS.Application.Features.Transport.Handlers;

public class SyncTransportFeeCommandHandler : IRequestHandler<SyncTransportFeeCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public SyncTransportFeeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(SyncTransportFeeCommand request, CancellationToken cancellationToken)
    {
        var studentFeesQuery = _context.StudentFees
            .Where(sf => sf.IsActive);

        if (request.EnrollmentId.HasValue)
        {
            studentFeesQuery = studentFeesQuery.Where(sf => sf.EnrollmentId == request.EnrollmentId.Value);
        }

        var studentFees = await studentFeesQuery.ToListAsync(cancellationToken);

        foreach (var fee in studentFees)
        {
            var activeAssignment = await _context.StudentTransportAssignments
                .Include(a => a.Route)
                .FirstOrDefaultAsync(a => a.EnrollmentId == fee.EnrollmentId && a.IsActive, cancellationToken);

            if (activeAssignment != null)
            {
                fee.TransportFeeAmount = activeAssignment.Route?.MonthlyFee ?? 0;
            }
            else
            {
                fee.TransportFeeAmount = 0;
            }
            fee.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
