using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMS.Application.Features.Payroll.DTOs;
using SMS.Application.Features.Payroll.Queries;

namespace SMS.API.Controllers;

/// <summary>
/// Teacher Payroll Management API
/// Handles payroll reports, bonus eligibility, and attendance summaries
/// </summary>
[Authorize(Policy = "PayrollAccess")]
[ApiController]
[Route("api/v1/[controller]")]
public class PayrollController : ControllerBase
{
    private readonly IMediator _mediator;

    public PayrollController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get teacher payroll report for a specified period
    /// </summary>
    /// <param name="startDate">Period start date (inclusive)</param>
    /// <param name="endDate">Period end date (inclusive)</param>
    /// <returns>Payroll period report with all teacher salary details</returns>
    [HttpGet("report")]
    public async Task<ActionResult<PayrollPeriodReportDto>> GetPayrollReport(
        [FromQuery] DateOnly startDate,
        [FromQuery] DateOnly endDate)
    {
        try
        {
            if (startDate > endDate)
            {
                return BadRequest(new { message = "Start date must be before or equal to end date" });
            }

            var query = new GetTeacherPayrollReportQuery
            {
                StartDate = startDate,
                EndDate = endDate
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving payroll report", error = ex.Message });
        }
    }

    /// <summary>
    /// Get bonus eligibility status for all teachers in a period
    /// </summary>
    /// <param name="startDate">Period start date (inclusive)</param>
    /// <param name="endDate">Period end date (inclusive)</param>
    /// <param name="bonusThresholdPercentage">Attendance threshold for bonus eligibility (default: 90)</param>
    /// <returns>List of teachers with bonus eligibility details</returns>
    [HttpGet("bonus-eligibility")]
    public async Task<ActionResult<List<BonusEligibilityDto>>> GetBonusEligibility(
        [FromQuery] DateOnly startDate,
        [FromQuery] DateOnly endDate,
        [FromQuery] decimal bonusThresholdPercentage = 90)
    {
        try
        {
            if (startDate > endDate)
            {
                return BadRequest(new { message = "Start date must be before or equal to end date" });
            }

            if (bonusThresholdPercentage < 0 || bonusThresholdPercentage > 100)
            {
                return BadRequest(new { message = "Bonus threshold percentage must be between 0 and 100" });
            }

            var query = new GetBonusEligibilityQuery
            {
                StartDate = startDate,
                EndDate = endDate,
                BonusThresholdPercentage = bonusThresholdPercentage
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving bonus eligibility", error = ex.Message });
        }
    }

    /// <summary>
    /// Get attendance summary for all teachers in a period
    /// </summary>
    /// <param name="startDate">Period start date (inclusive)</param>
    /// <param name="endDate">Period end date (inclusive)</param>
    /// <returns>List of teachers with attendance summary details</returns>
    [HttpGet("attendance-summary")]
    public async Task<ActionResult<List<TeacherAttendanceSummaryDto>>> GetAttendanceSummary(
        [FromQuery] DateOnly startDate,
        [FromQuery] DateOnly endDate)
    {
        try
        {
            if (startDate > endDate)
            {
                return BadRequest(new { message = "Start date must be before or equal to end date" });
            }

            var query = new GetTeacherAttendanceSummaryQuery
            {
                StartDate = startDate,
                EndDate = endDate
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving attendance summary", error = ex.Message });
        }
    }
}
