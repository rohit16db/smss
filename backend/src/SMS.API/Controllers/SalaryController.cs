using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMS.Application.Features.Salary.Commands;
using SMS.Application.Features.Salary.DTOs;
using SMS.Application.Features.Salary.Queries;

namespace SMS.API.Controllers;

/// <summary>
/// Teacher Salary Management API
/// Handles salary payments, tracking, and reporting
/// </summary>
[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class SalaryController : ControllerBase
{
    private readonly IMediator _mediator;

    public SalaryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get salary payment details by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<SalaryPaymentDto>> GetSalaryPayment(Guid id)
    {
        try
        {
            var query = new GetSalaryPaymentQuery { SalaryPaymentId = id };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        }
    }

    /// <summary>
    /// Get all salary payments for a specific period
    /// </summary>
    [HttpGet("period/report")]
    public async Task<ActionResult<SalaryPaymentReportDto>> GetSalaryPaymentsByPeriod(
        [FromQuery] DateOnly startDate,
        [FromQuery] DateOnly endDate)
    {
        try
        {
            if (startDate > endDate)
                return BadRequest(new { message = "Start date must be before or equal to end date" });

            var query = new GetSalaryPaymentsByPeriodQuery
            {
                StartDate = startDate,
                EndDate = endDate
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        }
    }

    /// <summary>
    /// Get salary history for a specific teacher
    /// </summary>
    [HttpGet("teacher/{teacherId}")]
    public async Task<ActionResult<SalaryHistoryDto>> GetTeacherSalaryHistory(
        Guid teacherId,
        [FromQuery] DateOnly? startDate,
        [FromQuery] DateOnly? endDate)
    {
        try
        {
            var query = new GetTeacherSalaryPaymentsQuery
            {
                TeacherId = teacherId,
                StartDate = startDate,
                EndDate = endDate
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        }
    }

    /// <summary>
    /// Get all pending salary payments
    /// </summary>
    [HttpGet("pending")]
    public async Task<ActionResult<List<SalaryPaymentDto>>> GetPendingSalaries(
        [FromQuery] DateOnly? asOfDate)
    {
        try
        {
            var query = new GetPendingSalaryPaymentsQuery { AsOfDate = asOfDate };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        }
    }

    /// <summary>
    /// Get salary summary for dashboard
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult<SalarySummaryDto>> GetSalarySummary(
        [FromQuery] int? month,
        [FromQuery] int? year)
    {
        try
        {
            var query = new GetSalarySummaryQuery { Month = month, Year = year };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        }
    }

    /// <summary>
    /// Create a new salary payment record
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<SalaryPaymentDto>> CreateSalaryPayment(
        [FromBody] CreateSalaryPaymentDto dto)
    {
        try
        {
            var command = new CreateSalaryPaymentCommand
            {
                TeacherId = dto.TeacherId,
                PeriodStartDate = dto.PeriodStartDate,
                PeriodEndDate = dto.PeriodEndDate,
                BaseSalary = dto.BaseSalary,
                Deductions = dto.Deductions,
                Bonus = dto.Bonus,
                ReferenceNumber = dto.ReferenceNumber,
                PaymentMethod = dto.PaymentMethod,
                Remarks = dto.Remarks
            };

            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetSalaryPayment), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        }
    }

    /// <summary>
    /// Create bulk salary payments for all active teachers
    /// </summary>
    [HttpPost("bulk")]
    public async Task<ActionResult<SalaryPaymentReportDto>> CreateBulkSalaryPayments(
        [FromBody] CreateBulkSalaryPaymentsCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        }
    }

    /// <summary>
    /// Update salary payment status
    /// </summary>
    [HttpPut("{id}/status")]
    public async Task<ActionResult<SalaryPaymentDto>> UpdateSalaryStatus(
        Guid id,
        [FromBody] UpdateSalaryPaymentStatusDto dto)
    {
        try
        {
            var command = new UpdateSalaryPaymentStatusCommand
            {
                SalaryPaymentId = id,
                Status = dto.Status,
                PaidDate = dto.PaidDate,
                ReferenceNumber = dto.ReferenceNumber,
                Remarks = dto.Remarks
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        }
    }

    /// <summary>
    /// Mark salary payment as paid
    /// </summary>
    [HttpPut("{id}/mark-paid")]
    public async Task<ActionResult<SalaryPaymentDto>> MarkSalaryAsPaid(
        Guid id,
        [FromBody] MarkSalaryAsPaidDto dto)
    {
        try
        {
            var command = new MarkSalaryAsPaidCommand
            {
                SalaryPaymentId = id,
                PaidDate = dto.PaidDate,
                PaymentMethod = dto.PaymentMethod,
                ReferenceNumber = dto.ReferenceNumber
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        }
    }

    /// <summary>
    /// Delete a salary payment (only if not paid)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteSalaryPayment(Guid id)
    {
        try
        {
            var command = new DeleteSalaryPaymentCommand { SalaryPaymentId = id };
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        }
    }
}

/// <summary>
/// DTO for marking salary as paid
/// </summary>
public class MarkSalaryAsPaidDto
{
    public DateOnly PaidDate { get; set; }
    public string? PaymentMethod { get; set; }
    public string? ReferenceNumber { get; set; }
}
