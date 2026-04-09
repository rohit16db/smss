using MediatR;
namespace SMS.Application.Features.Transport.Commands;

public class AssignStudentToTransportCommand : IRequest<Guid>
{
    public Guid EnrollmentId { get; set; }
    public Guid RouteId { get; set; }
    public Guid? RouteStopId { get; set; }
    public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;
}
