using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMS.Application.Features.Dashboard.DTOs;
using SMS.Application.Features.Dashboard.Queries;

namespace SMS.API.Controllers;

/// <summary>
/// Dashboard API - Provides aggregated data and reports for administrators
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get dashboard summary with KPIs, financial data, and attendance metrics
    /// </summary>
    /// <param name="startDate">Start date for data aggregation (default: 1 month ago)</param>
    /// <param name="endDate">End date for data aggregation (default: today)</param>
    /// <returns>Complete dashboard summary with cards, academic, financial, and attendance data</returns>
    /// <response code="200">Dashboard summary retrieved successfully</response>
    /// <response code="401">Unauthorized - JWT token required</response>
    /// <response code="500">Server error</response>
    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryResponseDto>> GetDashboardSummary(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetDashboardSummaryQuery
            {
                StartDate = startDate,
                EndDate = endDate
            };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            // Log exception here
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "An error occurred while retrieving dashboard summary", error = ex.Message });
        }
    }
}
