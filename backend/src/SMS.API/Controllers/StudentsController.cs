using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMS.Application.Students.Commands;
using SMS.Application.Students.DTOs;
using SMS.Application.Students.Queries;

namespace SMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AcademicAccess")]
public class StudentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<StudentsController> _logger;

    public StudentsController(IMediator mediator, ILogger<StudentsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all students with optional filtering and pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<StudentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<StudentDto>>> GetAll(
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? city = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetAllStudentsQuery
        {
            SearchTerm = searchTerm,
            IsActive = isActive,
            City = city,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get student by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(StudentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudentDto>> GetById(Guid id)
    {
        var query = new GetStudentByIdQuery { Id = id };
        var student = await _mediator.Send(query);
        return Ok(student);
    }

    /// <summary>
    /// Create a new student
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(StudentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<StudentDto>> Create([FromBody] CreateStudentRequest request)
    {
        var command = new CreateStudentCommand
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            DateOfBirth = request.DateOfBirth,
            Address = request.Address,
            City = request.City,
            State = request.State,
            PostalCode = request.PostalCode,
            GuardianName = request.GuardianName,
            GuardianPhone = request.GuardianPhone,
            GuardianEmail = request.GuardianEmail
        };

        var student = await _mediator.Send(command);
        _logger.LogInformation("Student {StudentId} created successfully", student.Id);
        return CreatedAtAction(nameof(GetById), new { id = student.Id }, student);
    }

    /// <summary>
    /// Update an existing student
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(StudentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<StudentDto>> Update(Guid id, [FromBody] UpdateStudentRequest request)
    {
        var command = new UpdateStudentCommand
        {
            Id = id,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            DateOfBirth = request.DateOfBirth,
            Address = request.Address,
            City = request.City,
            State = request.State,
            PostalCode = request.PostalCode,
            IsActive = request.IsActive,
            GuardianName = request.GuardianName,
            GuardianPhone = request.GuardianPhone,
            GuardianEmail = request.GuardianEmail
        };

        var student = await _mediator.Send(command);
        _logger.LogInformation("Student {StudentId} updated successfully", id);
        return Ok(student);
    }

    /// <summary>
    /// Delete a student (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteStudentCommand { Id = id };
        await _mediator.Send(command);
        _logger.LogInformation("Student {StudentId} deleted successfully", id);
        return NoContent();
    }

    /// <summary>
    /// Activate a student
    /// </summary>
    /// <param name="id">Student ID (GUID)</param>
    /// <returns>Success status</returns>
    [HttpPatch("{id}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(string id, CancellationToken cancellationToken)
    {
        var command = new ActivateStudentCommand { Id = id };

        try
        {
            await _mediator.Send(command, cancellationToken);
            _logger.LogInformation("Student {StudentId} activated successfully", id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to activate student {StudentId}", id);
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Deactivate a student
    /// </summary>
    /// <param name="id">Student ID (GUID)</param>
    /// <returns>Success status</returns>
    [HttpPatch("{id}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(string id, CancellationToken cancellationToken)
    {
        var command = new DeactivateStudentCommand { Id = id };

        try
        {
            await _mediator.Send(command, cancellationToken);
            _logger.LogInformation("Student {StudentId} deactivated successfully", id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to deactivate student {StudentId}", id);
            return NotFound(new { message = ex.Message });
        }
    }
}
