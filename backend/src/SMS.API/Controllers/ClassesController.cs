using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMS.Application.Features.Classes.Commands;
using SMS.Application.Features.Classes.DTOs;
using SMS.Application.Features.Classes.Queries;

namespace SMS.API.Controllers;

/// <summary>
/// API endpoints for managing classes and sections
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Policy = "AcademicViewAccess")]
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
    [Authorize(Policy = "AcademicAccess")]
    [ProducesResponseType(typeof(ClassDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateClassDto dto)
    {
        var command = new CreateClassCommand
        {
            Name = dto.Name,
            AcademicYearId = dto.AcademicYearId
        };

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Update an existing class
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Policy = "AcademicAccess")]
    [ProducesResponseType(typeof(ClassDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateClassDto dto)
    {
        var command = new UpdateClassCommand
        {
            Id = id,
            Name = dto.Name,
            AcademicYearId = dto.AcademicYearId,
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
    [Authorize(Policy = "AcademicAccess")]
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
    [Authorize(Policy = "AcademicAccess")]
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
    [Authorize(Policy = "AcademicAccess")]
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
    [Authorize(Policy = "AcademicAccess")]
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
    [Authorize(Policy = "AcademicAccess")]
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

    /// <summary>
    /// Get all students with roll numbers in a section
    /// </summary>
    [HttpGet("sections/{sectionId}/roll-numbers")]
    [ProducesResponseType(typeof(List<StudentSectionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRollNumbers(string sectionId)
    {
        var query = new GetStudentsWithRollNumbersQuery
        {
            SectionId = sectionId
        };

        var result = await _mediator.Send(query);
        
        if (result == null || result.Count == 0)
            return NotFound(new { message = "No students found in this section" });

        return Ok(result);
    }

    /// <summary>
    /// Auto-assign sequential roll numbers to all students in a section
    /// </summary>
    [HttpPost("sections/{sectionId}/auto-assign-roll-numbers")]
    [Authorize(Policy = "AcademicAccess")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AutoAssignRollNumbers(string sectionId)
    {
        var command = new AutoAssignRollNumbersCommand
        {
            SectionId = sectionId
        };

        try
        {
            await _mediator.Send(command);
            return Ok(new { message = "Roll numbers assigned successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update a student's roll number in a section
    /// </summary>
    [HttpPut("student-sections/{studentSectionId}/roll-number")]
    [Authorize(Policy = "AcademicAccess")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateRollNumber(string studentSectionId, [FromBody] UpdateRollNumberDto dto)
    {
        var command = new UpdateStudentRollNumberCommand
        {
            StudentSectionId = studentSectionId,
            RollNumber = dto.RollNumber
        };

        try
        {
            await _mediator.Send(command);
            return Ok(new { message = "Roll number updated successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Bulk update roll numbers for multiple students in a section
    /// </summary>
    [HttpPut("sections/{sectionId}/bulk-update-roll-numbers")]
    [Authorize(Policy = "AcademicAccess")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BulkUpdateRollNumbers(string sectionId, [FromBody] BulkUpdateRollNumbersDto dto)
    {
        var command = new BulkUpdateRollNumbersCommand
        {
            SectionId = sectionId,
            RollNumberUpdates = dto.RollNumberUpdates
        };

        try
        {
            await _mediator.Send(command);
            return Ok(new { message = "Roll numbers updated successfully" });
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

/// <summary>
/// DTO for updating a student's roll number
/// </summary>
public class UpdateRollNumberDto
{
    public int RollNumber { get; set; }
}

/// <summary>
/// DTO for bulk updating roll numbers
/// </summary>
public class BulkUpdateRollNumbersDto
{
    public Dictionary<string, int> RollNumberUpdates { get; set; } = new();
}
