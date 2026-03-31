using MediatR;

namespace SMS.Application.Features.Transport.Commands;

public class SyncTransportFeeCommand : IRequest<bool>
{
    public Guid? EnrollmentId { get; set; }
}
