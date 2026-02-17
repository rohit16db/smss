using MediatR;
using SMS.Application.Features.Dashboard.DTOs;

namespace SMS.Application.Features.Dashboard.Queries;

public class GetDashboardSummaryQuery : IRequest<DashboardSummaryResponseDto>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
