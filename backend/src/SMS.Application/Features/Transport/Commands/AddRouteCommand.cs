using MediatR;

namespace SMS.Application.Features.Transport.Commands;

public class AddRouteCommand : IRequest<Guid>
{
    public string RouteName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? VehicleId { get; set; }
    public decimal MonthlyFee { get; set; }
    public bool IsActive { get; set; } = true;
    public List<RouteStopCommandDto> Stops { get; set; } = new();
}

public class RouteStopCommandDto
{
    public string StopName { get; set; } = string.Empty;
    public string PickupTime { get; set; } = string.Empty;
    public string DropoffTime { get; set; } = string.Empty;
    public int Sequence { get; set; }
}
