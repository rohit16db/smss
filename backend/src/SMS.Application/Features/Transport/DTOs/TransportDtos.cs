namespace SMS.Application.Features.Transport.DTOs;

public class VehicleDto
{
    public Guid Id { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public string DriverPhone { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class TransportRouteDto
{
    public Guid Id { get; set; }
    public string RouteName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? VehicleId { get; set; }
    public string? VehicleRegistrationNumber { get; set; }
    public decimal MonthlyFee { get; set; }
    public bool IsActive { get; set; }
    public List<RouteStopDto> Stops { get; set; } = new();
}

public class RouteStopDto
{
    public Guid Id { get; set; }
    public string StopName { get; set; } = string.Empty;
    public string PickupTime { get; set; } = string.Empty;
    public string DropoffTime { get; set; } = string.Empty;
    public int Sequence { get; set; }
}

public class StudentTransportAssignmentDto
{
    public Guid Id { get; set; }
    public Guid EnrollmentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string EnrollmentNumber { get; set; } = string.Empty;
    public string ClassSection { get; set; } = string.Empty;
    public Guid RouteId { get; set; }
    public string RouteName { get; set; } = string.Empty;
    public string StopName { get; set; } = string.Empty;
    public decimal MonthlyFee { get; set; }
    public string VehicleReg { get; set; } = string.Empty;
    public DateTime EffectiveDate { get; set; }
    public bool IsActive { get; set; }
}
