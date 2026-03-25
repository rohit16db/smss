using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMS.Application.Features.Holidays.Commands;
using SMS.Application.Features.Holidays.DTOs;
using SMS.Application.Features.Holidays.Queries;

namespace SMS.API.Controllers;

/// <summary>
/// Controller for managing school holidays
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class HolidaysController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<HolidaysController> _logger;

    public HolidaysController(IMediator mediator, ILogger<HolidaysController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all holidays with optional filters
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedHolidayListDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllHolidays(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? academicYearId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? type = null)
    {
        try
        {
            var query = new GetAllHolidaysQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                AcademicYearId = academicYearId,
                StartDate = startDate,
                EndDate = endDate,
                Type = type
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving holidays");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving holidays");
        }
    }

    /// <summary>
    /// Get holiday by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(HolidayDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHolidayById(string id)
    {
        try
        {
            var query = new GetHolidayByIdQuery { Id = id };
            var result = await _mediator.Send(query);

            if (result == null)
                return NotFound($"Holiday with ID {id} not found");

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving holiday");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving holiday");
        }
    }

    /// <summary>
    /// Get holidays for a specific month
    /// </summary>
    [HttpGet("month/{year}/{month}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<HolidayDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHolidaysByMonth(int year, int month)
    {
        try
        {
            var query = new GetHolidaysByMonthQuery { Year = year, Month = month };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving holidays for month");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving holidays");
        }
    }

    /// <summary>
    /// Create a new holiday
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(HolidayDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateHoliday([FromBody] CreateHolidayDto dto)
    {
        try
        {
            var command = new CreateHolidayCommand
            {
                Name = dto.Name,
                HolidayDate = dto.HolidayDate,
                Description = dto.Description,
                Type = dto.Type,
                AcademicYearId = dto.AcademicYearId
            };

            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetHolidayById), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating holiday");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error creating holiday");
        }
    }

    /// <summary>
    /// Update an existing holiday
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(HolidayDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateHoliday(string id, [FromBody] UpdateHolidayDto dto)
    {
        try
        {
            var command = new UpdateHolidayCommand
            {
                Id = id,
                Name = dto.Name,
                HolidayDate = dto.HolidayDate,
                Description = dto.Description,
                Type = dto.Type,
                AcademicYearId = dto.AcademicYearId
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Holiday with ID {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating holiday");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error updating holiday");
        }
    }

    /// <summary>
    /// Delete a holiday
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteHoliday(string id)
    {
        try
        {
            var command = new DeleteHolidayCommand { Id = id };
            await _mediator.Send(command);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Holiday with ID {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting holiday");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting holiday");
        }
    }
}
