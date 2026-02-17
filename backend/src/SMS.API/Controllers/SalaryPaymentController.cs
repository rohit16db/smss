using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMS.Application.Features.Salary.Commands;
using SMS.Application.Features.Salary.DTOs;
using SMS.Application.Features.Salary.Queries;
using SMS.Domain.Entities;
using SMS.Domain.Enums;

namespace SMS.API.Controllers;

[ApiController]
[Route("api/v1/salary-management")]
[Authorize]
public class SalaryPaymentController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<SalaryPaymentController> _logger;

    public SalaryPaymentController(IMediator mediator, ILogger<SalaryPaymentController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all salary payments with optional filters
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<SalaryPaymentDto>>> GetAllSalaryPayments(
        [FromQuery] string? status = null,
        [FromQuery] Guid? teacherId = null,
        [FromQuery] DateTime? periodStartDate = null,
        [FromQuery] DateTime? periodEndDate = null)
    {
        try
        {
            var query = new GetAllSalaryPaymentsQuery
            {
                Status = status,
                TeacherId = teacherId,
                PeriodStartDate = periodStartDate,
                PeriodEndDate = periodEndDate
            };

            var payments = await _mediator.Send(query);
            return Ok(payments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving salary payments");
            return StatusCode(500, new { message = "An error occurred while retrieving salary payments" });
        }
    }

    /// <summary>
    /// Get salary payment by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<SalaryPaymentDto>> GetSalaryPaymentById(Guid id)
    {
        try
        {
            var query = new GetSalaryPaymentQuery { SalaryPaymentId = id };
            var payment = await _mediator.Send(query);
            return Ok(payment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving salary payment with ID: {Id}", id);
            return NotFound(new { message = $"Salary payment with ID {id} not found" });
        }
    }

    /// <summary>
    /// Get all salary payments for a specific teacher
    /// </summary>
    [HttpGet("teacher/{teacherId}")]
    public async Task<ActionResult<SalaryHistoryDto>> GetTeacherSalaryPayments(Guid teacherId)
    {
        try
        {
            var query = new GetTeacherSalaryPaymentsQuery { TeacherId = teacherId };
            var payments = await _mediator.Send(query);
            return Ok(payments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving salary payments for teacher: {TeacherId}", teacherId);
            return StatusCode(500, new { message = "An error occurred while retrieving teacher salary payments" });
        }
    }

    /// <summary>
    /// Get salary payments summary
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult<SalaryPaymentSummaryDto>> GetSalaryPaymentsSummary(
        [FromQuery] DateTime? periodStartDate = null,
        [FromQuery] DateTime? periodEndDate = null)
    {
        try
        {
            var query = new GetSalaryPaymentsSummaryQuery
            {
                PeriodStartDate = periodStartDate,
                PeriodEndDate = periodEndDate
            };

            var summary = await _mediator.Send(query);
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving salary payments summary");
            return StatusCode(500, new { message = "An error occurred while retrieving salary payments summary" });
        }
    }

    /// <summary>
    /// Update salary payment status
    /// </summary>
    [HttpPut("{id}/status")]
    public async Task<ActionResult<SalaryPaymentDto>> UpdateSalaryPaymentStatus(
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

            var payment = await _mediator.Send(command);
            return Ok(payment);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when updating salary payment status");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating salary payment status with ID: {Id}", id);
            return StatusCode(500, new { message = "An error occurred while updating salary payment status" });
        }
    }

    /// <summary>
    /// Mark salary payment as paid
    /// </summary>
    [HttpPut("{id}/pay")]
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

            var payment = await _mediator.Send(command);
            return Ok(payment);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when marking salary as paid");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking salary payment as paid with ID: {Id}", id);
            return StatusCode(500, new { message = "An error occurred while marking salary as paid" });
        }
    }

    /// <summary>
    /// Update salary payment details (amounts, deductions)
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<SalaryPaymentDto>> UpdateSalaryPayment(
        Guid id,
        [FromBody] UpdateSalaryPaymentDto dto)
    {
        try
        {
            var command = new UpdateSalaryPaymentCommand
            {
                SalaryPaymentId = id,
                BaseSalary = dto.BaseSalary,
                Deductions = dto.Deductions,
                Bonus = dto.Bonus,
                Remarks = dto.Remarks
            };

            var payment = await _mediator.Send(command);
            return Ok(payment);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when updating salary payment");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating salary payment with ID: {Id}", id);
            return StatusCode(500, new { message = "An error occurred while updating salary payment" });
        }
    }

    /// <summary>
    /// Delete salary payment
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteSalaryPayment(Guid id)
    {
        try
        {
            var command = new DeleteSalaryPaymentCommand { SalaryPaymentId = id };
            await _mediator.Send(command);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation when deleting salary payment");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting salary payment with ID: {Id}", id);
            return StatusCode(500, new { message = "An error occurred while deleting salary payment" });
        }
    }
}
