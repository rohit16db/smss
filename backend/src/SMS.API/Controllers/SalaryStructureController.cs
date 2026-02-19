using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMS.Application.Features.Salary.Commands;
using SMS.Application.Features.Salary.DTOs;
using SMS.Application.Features.Salary.Queries;

namespace SMS.API.Controllers;

/// <summary>
/// Salary Structure Management API
/// Handles salary structure definitions and teacher assignments
/// </summary>
[Authorize(Policy = "SalaryManageAccess")]
[ApiController]
[Route("api/v1/[controller]")]
public class SalaryStructureController : ControllerBase
{
    private readonly IMediator _mediator;

    public SalaryStructureController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all salary structures
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<SalaryStructureDto>>> GetAllSalaryStructures(
        [FromQuery] bool? isActive)
    {
        try
        {
            var query = new GetAllSalaryStructuresQuery { IsActive = isActive };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        }
    }

    /// <summary>
    /// Get salary structure by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<SalaryStructureDto>> GetSalaryStructureById(Guid id)
    {
        try
        {
            var query = new GetSalaryStructureByIdQuery { Id = id };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        }
    }

    /// <summary>
    /// Get salary structures applicable for a teacher
    /// </summary>
    [HttpGet("applicable/{teacherId}")]
    public async Task<ActionResult<List<SalaryStructureDto>>> GetApplicableSalaryStructures(Guid teacherId)
    {
        try
        {
            var query = new GetApplicableSalaryStructuresQuery { TeacherId = teacherId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        }
    }

    /// <summary>
    /// Get current salary structure for a teacher
    /// </summary>
    [HttpGet("teacher/{teacherId}/current")]
    public async Task<ActionResult<TeacherSalaryAssignmentDto>> GetTeacherCurrentSalaryStructure(Guid teacherId)
    {
        try
        {
            var query = new GetTeacherCurrentSalaryStructureQuery { TeacherId = teacherId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        }
    }

    /// <summary>
    /// Get all teachers with their assigned salary structures
    /// </summary>
    [HttpGet("teachers/assignments")]
    public async Task<ActionResult<List<TeacherSalaryAssignmentDto>>> GetTeachersWithSalaryStructures(
        [FromQuery] bool? isActive)
    {
        try
        {
            var query = new GetTeachersWithSalaryStructuresQuery { IsActive = isActive };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        }
    }

    /// <summary>
    /// Create a new salary structure
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<SalaryStructureDto>> CreateSalaryStructure(
        [FromBody] CreateSalaryStructureDto dto)
    {
        try
        {
            var command = new CreateSalaryStructureCommand
            {
                Name = dto.Name,
                Description = dto.Description,
                BaseSalary = dto.BaseSalary,
                HRA = dto.HRA,
                DA = dto.DA,
                MedicalAllowance = dto.MedicalAllowance,
                ConveyanceAllowance = dto.ConveyanceAllowance,
                OtherAllowances = dto.OtherAllowances,
                StandardDeduction = dto.StandardDeduction,
                MinExperienceYears = dto.MinExperienceYears,
                ApplicableQualifications = dto.ApplicableQualifications,
                EffectiveFromDate = dto.EffectiveFromDate,
                EffectiveToDate = dto.EffectiveToDate
            };

            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetSalaryStructureById), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing salary structure
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<SalaryStructureDto>> UpdateSalaryStructure(
        Guid id,
        [FromBody] UpdateSalaryStructureDto dto)
    {
        try
        {
            if (id != dto.Id)
                return BadRequest(new { message = "ID mismatch" });

            var command = new UpdateSalaryStructureCommand
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
                BaseSalary = dto.BaseSalary,
                HRA = dto.HRA,
                DA = dto.DA,
                MedicalAllowance = dto.MedicalAllowance,
                ConveyanceAllowance = dto.ConveyanceAllowance,
                OtherAllowances = dto.OtherAllowances,
                StandardDeduction = dto.StandardDeduction,
                MinExperienceYears = dto.MinExperienceYears,
                ApplicableQualifications = dto.ApplicableQualifications,
                EffectiveFromDate = dto.EffectiveFromDate,
                EffectiveToDate = dto.EffectiveToDate
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        }
    }

    /// <summary>
    /// Delete a salary structure
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteSalaryStructure(Guid id)
    {
        try
        {
            var command = new DeleteSalaryStructureCommand { Id = id };
            await _mediator.Send(command);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        }
    }

    /// <summary>
    /// Assign salary structure to a teacher
    /// </summary>
    [HttpPost("assign-to-teacher")]
    public async Task<ActionResult<TeacherSalaryAssignmentDto>> AssignSalaryStructureToTeacher(
        [FromBody] AssignSalaryStructureDto dto)
    {
        try
        {
            var command = new AssignSalaryStructureToTeacherCommand
            {
                TeacherId = dto.TeacherId,
                SalaryStructureId = dto.SalaryStructureId,
                EffectiveDate = dto.EffectiveDate
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        }
    }

    /// <summary>
    /// Bulk create salary payments from structures for all teachers
    /// </summary>
    [HttpPost("bulk-create-salaries")]
    public async Task<ActionResult<SalaryPaymentReportDto>> BulkCreateSalaryPayments(
        [FromBody] BulkCreateFromStructureDto dto)
    {
        try
        {
            if (dto.PeriodStartDate > dto.PeriodEndDate)
                return BadRequest(new { message = "Period start date must be before or equal to end date" });

            var command = new BulkCreateSalaryFromStructuresCommand
            {
                PeriodStartDate = dto.PeriodStartDate,
                PeriodEndDate = dto.PeriodEndDate,
                FixedDeductions = dto.FixedDeductions ?? 0
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        }
    }
}
