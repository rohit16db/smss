namespace SMS.Domain.Entities;

public class StudentTransportAssignment : BaseEntity
{
    public Guid EnrollmentId { get; set; }
    public Guid RouteId { get; set; }
    public Guid? RouteStopId { get; set; }
    public DateTime EffectiveDate { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public Enrollment? Enrollment { get; set; }
    public TransportRoute? Route { get; set; }
    public RouteStop? RouteStop { get; set; }
}
