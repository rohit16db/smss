using MediatR;

namespace SMS.Application.Features.Transport.Commands;

public class UpdateRouteCommand : IRequest<bool>
{
    public Guid Id { get; set; }
    public string RouteName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? VehicleId { get; set; }
    public decimal MonthlyFee { get; set; }
    public bool IsActive { get; set; }
    public List<RouteStopUpdateDto> Stops { get; set; } = new();
}

public class RouteStopUpdateDto
{
    public Guid? Id { get; set; } // Null for new stops
    public string StopName { get; set; } = string.Empty;
    public string PickupTime { get; set; } = string.Empty;
    public string DropoffTime { get; set; } = string.Empty;
    public int Sequence { get; set; }
}
