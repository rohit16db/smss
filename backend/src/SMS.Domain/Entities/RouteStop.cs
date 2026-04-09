namespace SMS.Domain.Entities;

public class RouteStop : BaseEntity
{
    public Guid RouteId { get; set; }
    public string StopName { get; set; } = string.Empty;
    public string PickupTime { get; set; } = string.Empty;
    public string DropoffTime { get; set; } = string.Empty;
    public int Sequence { get; set; }

    // Navigation properties
    public TransportRoute? Route { get; set; }
}
