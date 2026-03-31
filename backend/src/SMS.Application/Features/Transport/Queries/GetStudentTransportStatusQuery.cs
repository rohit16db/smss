using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Transport.DTOs;

namespace SMS.Application.Features.Transport.Queries;

public class GetStudentTransportStatusQuery : IRequest<StudentTransportAssignmentDto?>
{
    public Guid Id { get; set; }
}

public class GetStudentTransportStatusHandler : IRequestHandler<GetStudentTransportStatusQuery, StudentTransportAssignmentDto?>
{
    private readonly IApplicationDbContext _context;

    public GetStudentTransportStatusHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StudentTransportAssignmentDto?> Handle(GetStudentTransportStatusQuery request, CancellationToken cancellationToken)
    {
        var assignment = await _context.StudentTransportAssignments
            .Include(a => a.Route)
                .ThenInclude(r => r.Vehicle)
            .Include(a => a.RouteStop)
            .Include(a => a.Enrollment)
            .Where(a => (a.EnrollmentId == request.Id || a.Enrollment!.StudentId == request.Id) && a.IsActive)
            .OrderByDescending(a => a.EffectiveDate)
            .Select(a => new StudentTransportAssignmentDto
            {
                Id = a.Id,
                EnrollmentId = a.EnrollmentId,
                RouteId = a.RouteId,
                RouteName = a.Route!.RouteName,
                StopName = a.RouteStop != null ? a.RouteStop.StopName : "Multiple Stops",
                MonthlyFee = a.Route.MonthlyFee,
                VehicleReg = a.Route.Vehicle != null ? a.Route.Vehicle.RegistrationNumber : "N/A",
                EffectiveDate = a.EffectiveDate,
                IsActive = a.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);

        return assignment;
    }
}
