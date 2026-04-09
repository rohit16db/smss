using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Transport.Commands;

namespace SMS.Application.Features.Transport.Handlers;

public class UpdateVehicleCommandHandler : IRequestHandler<UpdateVehicleCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public UpdateVehicleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateVehicleCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == request.Id, cancellationToken);

        if (vehicle == null) return false;

        vehicle.RegistrationNumber = request.RegistrationNumber;
        vehicle.Model = request.Model;
        vehicle.Capacity = request.Capacity;
        vehicle.DriverName = request.DriverName;
        vehicle.DriverPhone = request.DriverPhone;
        vehicle.IsActive = request.IsActive;
        vehicle.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
