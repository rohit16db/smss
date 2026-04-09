using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Transport.Commands;

namespace SMS.Application.Features.Transport.Handlers;

public class DeleteRouteCommandHandler : IRequestHandler<DeleteRouteCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteRouteCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteRouteCommand request, CancellationToken cancellationToken)
    {
        var route = await _context.TransportRoutes
            .Include(r => r.Stops)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (route == null) return false;

        // Check for active student assignments
        var hasActiveAssignments = await _context.StudentTransportAssignments
            .AnyAsync(a => a.RouteId == request.Id && a.IsActive, cancellationToken);

        if (hasActiveAssignments)
        {
            // Instead of deleting, just deactivate
            route.IsActive = false;
        }
        else
        {
            _context.TransportRoutes.Remove(route);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public class DeleteVehicleCommandHandler : IRequestHandler<DeleteVehicleCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteVehicleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteVehicleCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken);

        if (vehicle == null) return false;

        // Check for active routes using this vehicle
        var hasActiveRoutes = await _context.TransportRoutes
            .AnyAsync(r => r.VehicleId == request.Id && r.IsActive, cancellationToken);

        if (hasActiveRoutes)
        {
            // Instead of deleting, just deactivate
            vehicle.IsActive = false;
        }
        else
        {
            _context.Vehicles.Remove(vehicle);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
