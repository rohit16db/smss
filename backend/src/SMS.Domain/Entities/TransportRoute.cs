namespace SMS.Domain.Entities;

public class TransportRoute : BaseEntity
{
    public string RouteName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? VehicleId { get; set; }
    public decimal MonthlyFee { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public Vehicle? Vehicle { get; set; }
    public ICollection<RouteStop> Stops { get; set; } = new List<RouteStop>();
    public ICollection<StudentTransportAssignment> Assignments { get; set; } = new List<StudentTransportAssignment>();
}
