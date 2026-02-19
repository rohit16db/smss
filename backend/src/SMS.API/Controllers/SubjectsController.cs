using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMS.Application.Features.Subjects.Commands;
using SMS.Application.Features.Subjects.DTOs;
using SMS.Application.Features.Subjects.Queries;
using SMS.Domain.Exceptions;

namespace SMS.API.Controllers;

[Authorize(Policy = "AcademicAccess")]
[ApiController]
[Route("api/subjects")]
public class SubjectsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<SubjectsController> _logger;

    public SubjectsController(IMediator mediator, ILogger<SubjectsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all subjects with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedSubjectListDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? searchTerm = null, [FromQuery] bool? isActive = null)
    {
        var query = new GetAllSubjectsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            IsActive = isActive
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get a subject by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SubjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id)
    {
        var query = new GetSubjectByIdQuery { Id = id };
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound(new { message = "Subject not found" });

        return Ok(result);
    }

    /// <summary>
    /// Get all active subjects (for dropdowns)
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(List<SubjectListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActive()
    {
        var query = new GetActiveSubjectsQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Create a new subject
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(SubjectDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateSubjectDto dto)
    {
        try
        {
            var command = new CreateSubjectCommand
            {
                Name = dto.Name,
                Code = dto.Code,
                Description = dto.Description,
                Credits = dto.Credits,
                DisplayOrder = dto.DisplayOrder
            };

            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error creating subject");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating subject");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error creating subject" });
        }
    }

    /// <summary>
    /// Update an existing subject
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(SubjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateSubjectDto dto)
    {
        try
        {
            var command = new UpdateSubjectCommand
            {
                Id = id,
                Name = dto.Name,
                Code = dto.Code,
                Description = dto.Description,
                Credits = dto.Credits,
                IsActive = dto.IsActive,
                DisplayOrder = dto.DisplayOrder
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Subject not found");
            return NotFound(new { message = ex.Message });
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error updating subject");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating subject");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error updating subject" });
        }
    }

    /// <summary>
    /// Delete a subject
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            var command = new DeleteSubjectCommand { Id = id };
            await _mediator.Send(command);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Subject not found");
            return NotFound(new { message = ex.Message });
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error deleting subject");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting subject");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error deleting subject" });
        }
    }
}
