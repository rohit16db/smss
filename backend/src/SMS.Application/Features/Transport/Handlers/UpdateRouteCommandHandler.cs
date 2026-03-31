using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Transport.Commands;
using SMS.Domain.Entities;

namespace SMS.Application.Features.Transport.Handlers;

public class UpdateRouteCommandHandler : IRequestHandler<UpdateRouteCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateRouteCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateRouteCommand request, CancellationToken cancellationToken)
    {
        var route = await _context.TransportRoutes
            .Include(r => r.Stops)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (route == null) return false;

        route.RouteName = request.RouteName;
        route.Description = request.Description;
        route.VehicleId = request.VehicleId;
        route.MonthlyFee = request.MonthlyFee;
        route.IsActive = request.IsActive;
        route.UpdatedAt = DateTime.UtcNow;

        // Synchronize Stops
        var existingStopIds = route.Stops.Select(s => s.Id).ToList();
        var incomingStopIds = request.Stops.Where(s => s.Id.HasValue).Select(s => s.Id!.Value).ToList();

        // 1. Remove stops that are not in the incoming request
        var stopsToRemove = route.Stops.Where(s => !incomingStopIds.Contains(s.Id)).ToList();
        foreach (var stop in stopsToRemove)
        {
            route.Stops.Remove(stop);
        }

        // 2. Update or Add stops
        foreach (var stopDto in request.Stops)
        {
            if (stopDto.Id.HasValue && existingStopIds.Contains(stopDto.Id.Value))
            {
                // Update existing
                var existingStop = route.Stops.First(s => s.Id == stopDto.Id.Value);
                existingStop.StopName = stopDto.StopName;
                existingStop.PickupTime = stopDto.PickupTime;
                existingStop.DropoffTime = stopDto.DropoffTime;
                existingStop.Sequence = stopDto.Sequence;
            }
            else
            {
                // Add new
                route.Stops.Add(new RouteStop
                {
                    Id = Guid.NewGuid(),
                    StopName = stopDto.StopName,
                    PickupTime = stopDto.PickupTime,
                    DropoffTime = stopDto.DropoffTime,
                    Sequence = stopDto.Sequence,
                    RouteId = route.Id
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
