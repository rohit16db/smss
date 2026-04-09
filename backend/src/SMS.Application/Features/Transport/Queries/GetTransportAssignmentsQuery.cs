using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Transport.DTOs;
using SMS.Domain.Entities;

namespace SMS.Application.Features.Transport.Queries;

public class GetTransportAssignmentsQuery : IRequest<List<StudentTransportAssignmentDto>>
{
    public bool ActiveOnly { get; set; } = true;
}

public class GetTransportAssignmentsHandler : IRequestHandler<GetTransportAssignmentsQuery, List<StudentTransportAssignmentDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTransportAssignmentsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<StudentTransportAssignmentDto>> Handle(GetTransportAssignmentsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.StudentTransportAssignments
            .Include(a => a.Enrollment)
                .ThenInclude(e => e.Student)
            .Include(a => a.Enrollment)
                .ThenInclude(e => e.Class)
            .Include(a => a.Enrollment)
                .ThenInclude(e => e.Section)
            .Include(a => a.Route)
                .ThenInclude(r => r.Vehicle)
            .Include(a => a.RouteStop)
            .AsQueryable();

        if (request.ActiveOnly)
        {
            query = query.Where(a => a.IsActive);
        }

        var assignments = await query
            .OrderByDescending(a => a.EffectiveDate)
            .Select(a => new StudentTransportAssignmentDto
            {
                Id = a.Id,
                EnrollmentId = a.EnrollmentId,
                StudentName = $"{a.Enrollment.Student.FirstName} {a.Enrollment.Student.LastName}",
                EnrollmentNumber = a.Enrollment.Student.EnrollmentNumber,
                ClassSection = $"{a.Enrollment.Class.Name} - {a.Enrollment.Section.SectionName}",
                RouteId = a.RouteId,
                RouteName = a.Route.RouteName,
                StopName = a.RouteStop != null ? a.RouteStop.StopName : "Multiple Stops",
                MonthlyFee = a.Route.MonthlyFee,
                VehicleReg = a.Route.Vehicle != null ? a.Route.Vehicle.RegistrationNumber : "N/A",
                EffectiveDate = a.EffectiveDate,
                IsActive = a.IsActive
            })
            .ToListAsync(cancellationToken);

        return assignments;
    }
}
