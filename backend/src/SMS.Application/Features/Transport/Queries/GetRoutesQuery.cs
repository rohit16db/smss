using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Transport.DTOs;

namespace SMS.Application.Features.Transport.Queries;

public class GetRoutesQuery : IRequest<List<TransportRouteDto>>
{
    public bool? IsActive { get; set; }
}

public class GetRoutesQueryHandler : IRequestHandler<GetRoutesQuery, List<TransportRouteDto>>
{
    private readonly IApplicationDbContext _context;

    public GetRoutesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TransportRouteDto>> Handle(GetRoutesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.TransportRoutes
            .Include(r => r.Vehicle)
            .Include(r => r.Stops)
            .AsQueryable();

        if (request.IsActive.HasValue)
        {
            query = query.Where(r => r.IsActive == request.IsActive.Value);
        }

        return await query.Select(r => new TransportRouteDto
        {
            Id = r.Id,
            RouteName = r.RouteName,
            Description = r.Description,
            VehicleId = r.VehicleId,
            VehicleRegistrationNumber = r.Vehicle != null ? r.Vehicle.RegistrationNumber : null,
            MonthlyFee = r.MonthlyFee,
            IsActive = r.IsActive,
            Stops = r.Stops.OrderBy(s => s.Sequence).Select(s => new RouteStopDto
            {
                Id = s.Id,
                StopName = s.StopName,
                PickupTime = s.PickupTime,
                DropoffTime = s.DropoffTime,
                Sequence = s.Sequence
            }).ToList()
        }).ToListAsync(cancellationToken);
    }
}
