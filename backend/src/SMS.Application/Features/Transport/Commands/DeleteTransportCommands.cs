using MediatR;

namespace SMS.Application.Features.Transport.Commands;

public class DeleteRouteCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}

public class DeleteVehicleCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}
