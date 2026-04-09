using MediatR;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Transport.Commands;
using SMS.Domain.Entities;

namespace SMS.Application.Features.Transport.Handlers;

public class AddVehicleCommandHandler : IRequestHandler<AddVehicleCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public AddVehicleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(AddVehicleCommand request, CancellationToken cancellationToken)
    {
        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            RegistrationNumber = request.RegistrationNumber,
            Model = request.Model,
            Capacity = request.Capacity,
            DriverName = request.DriverName,
            DriverPhone = request.DriverPhone,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync(cancellationToken);

        return vehicle.Id;
    }
}
