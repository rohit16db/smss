using MediatR;
using Microsoft.AspNetCore.Mvc;
using SMS.Application.Features.Classes.Commands;
using SMS.Application.Features.Classes.DTOs;
using SMS.Application.Features.Classes.Queries;

namespace SMS.API.Controllers;

/// <summary>
/// API endpoints for managing classes and sections
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ClassesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ClassesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all classes with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedClassListDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? searchTerm = null, [FromQuery] bool? isActive = null)
    {
        var query = new GetAllClassesQuery
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
    /// Get a specific class by ID with all sections
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ClassDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id)
    {
        var query = new GetClassByIdQuery { Id = id };
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound(new { message = $"Class with ID {id} not found" });

        return Ok(result);
    }

    /// <summary>
    /// Create a new class
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ClassDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateClassDto dto)
    {
        var command = new CreateClassCommand
        {
            Name = dto.Name,
            AcademicYear = dto.AcademicYear
        };

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Update an existing class
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ClassDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateClassDto dto)
    {
        var command = new UpdateClassCommand
        {
            Id = id,
            Name = dto.Name,
            AcademicYear = dto.AcademicYear,
            IsActive = dto.IsActive
        };

        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete a class
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id)
    {
        var command = new DeleteClassCommand { Id = id };

        try
        {
            await _mediator.Send(command);
            return Ok(new { message = "Class deleted successfully" });
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get all sections for a specific class
    /// </summary>
    [HttpGet("{classId}/sections")]
    [ProducesResponseType(typeof(List<SectionListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSectionsByClass(string classId)
    {
        var query = new GetSectionsByClassIdQuery { ClassId = classId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get section details by ID
    /// </summary>
    [HttpGet("sections/{id}")]
    [ProducesResponseType(typeof(SectionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSectionById(string id)
    {
        var query = new GetSectionByIdQuery { Id = id };
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound(new { message = $"Section with ID {id} not found" });

        return Ok(result);
    }

    /// <summary>
    /// Create a new section within a class
    /// </summary>
    [HttpPost("sections")]
    [ProducesResponseType(typeof(SectionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSection([FromBody] CreateSectionDto dto)
    {
        var command = new CreateSectionCommand
        {
            ClassId = dto.ClassId,
            SectionName = dto.SectionName
        };

        try
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetSectionById), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update a section
    /// </summary>
    [HttpPut("sections/{id}")]
    [ProducesResponseType(typeof(SectionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSection(string id, [FromBody] UpdateSectionDto dto)
    {
        var command = new UpdateSectionCommand
        {
            Id = id,
            SectionName = dto.SectionName,
            IsActive = dto.IsActive
        };

        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete a section
    /// </summary>
    [HttpDelete("sections/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSection(string id)
    {
        var command = new DeleteSectionCommand { Id = id };

        try
        {
            await _mediator.Send(command);
            return Ok(new { message = "Section deleted successfully" });
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get student's section history (all enrollments)
    /// </summary>
    [HttpGet("students/{studentId}/section-history")]
    [ProducesResponseType(typeof(StudentSectionHistoryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStudentSectionHistory(string studentId)
    {
        var query = new GetStudentSectionHistoryQuery { StudentId = studentId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get student's current section
    /// </summary>
    [HttpGet("students/{studentId}/current-section")]
    [ProducesResponseType(typeof(StudentSectionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStudentCurrentSection(string studentId)
    {
        var query = new GetStudentCurrentSectionQuery { StudentId = studentId };
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound(new { message = $"No current section found for student {studentId}" });

        return Ok(result);
    }

    /// <summary>
    /// Move a student to a different section
    /// </summary>
    [HttpPost("students/{studentId}/move-section")]
    [ProducesResponseType(typeof(StudentSectionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> MoveStudentToSection(string studentId, [FromBody] MoveStudentSectionDto dto)
    {
        var command = new MoveStudentSectionCommand
        {
            StudentId = studentId,
            NewSectionId = dto.NewSectionId
        };

        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

/// <summary>
/// DTO for moving student to a different section
/// </summary>
public class MoveStudentSectionDto
{
    public string NewSectionId { get; set; } = string.Empty;
}
