using MediatR;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Transport.Commands;
using SMS.Domain.Entities;

namespace SMS.Application.Features.Transport.Handlers;

public class AddRouteCommandHandler : IRequestHandler<AddRouteCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public AddRouteCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(AddRouteCommand request, CancellationToken cancellationToken)
    {
        var route = new TransportRoute
        {
            Id = Guid.NewGuid(),
            RouteName = request.RouteName,
            Description = request.Description,
            VehicleId = request.VehicleId,
            MonthlyFee = request.MonthlyFee,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Stops = request.Stops.Select(s => new RouteStop
            {
                Id = Guid.NewGuid(),
                StopName = s.StopName,
                PickupTime = s.PickupTime,
                DropoffTime = s.DropoffTime,
                Sequence = s.Sequence
            }).ToList()
        };

        _context.TransportRoutes.Add(route);
        await _context.SaveChangesAsync(cancellationToken);

        return route.Id;
    }
}
