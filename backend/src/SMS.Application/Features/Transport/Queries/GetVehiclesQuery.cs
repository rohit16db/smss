using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Transport.DTOs;

namespace SMS.Application.Features.Transport.Queries;

public class GetVehiclesQuery : IRequest<List<VehicleDto>>
{
    public bool? IsActive { get; set; }
}

public class GetVehiclesQueryHandler : IRequestHandler<GetVehiclesQuery, List<VehicleDto>>
{
    private readonly IApplicationDbContext _context;

    public GetVehiclesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<VehicleDto>> Handle(GetVehiclesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Vehicles.AsQueryable();

        if (request.IsActive.HasValue)
        {
            query = query.Where(v => v.IsActive == request.IsActive.Value);
        }

        return await query.Select(v => new VehicleDto
        {
            Id = v.Id,
            RegistrationNumber = v.RegistrationNumber,
            Model = v.Model,
            Capacity = v.Capacity,
            DriverName = v.DriverName,
            DriverPhone = v.DriverPhone,
            IsActive = v.IsActive
        }).ToListAsync(cancellationToken);
    }
}
