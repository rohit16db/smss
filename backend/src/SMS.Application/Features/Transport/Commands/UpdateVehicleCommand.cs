using MediatR;

namespace SMS.Application.Features.Transport.Commands;

public class UpdateVehicleCommand : IRequest<bool>
{
    public Guid Id { get; set; }
    public string RegistrationNumber { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public string DriverPhone { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
