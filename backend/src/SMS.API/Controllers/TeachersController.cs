using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMS.API.Services;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Teachers.Commands;
using SMS.Application.Features.Teachers.DTOs;
using SMS.Application.Features.Teachers.Queries;
using System.ComponentModel.DataAnnotations;

namespace SMS.API.Controllers;

/// <summary>
/// API controller for Teacher management
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AcademicAccess")]
public class TeachersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IImageUploadService _imageUploadService;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<TeachersController> _logger;

    public TeachersController(IMediator mediator, IImageUploadService imageUploadService, IApplicationDbContext context, ILogger<TeachersController> logger)
    {
        _mediator = mediator;
        _imageUploadService = imageUploadService;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all teachers with pagination
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <param name="searchTerm">Optional search term for name, email, or phone</param>
    /// <param name="isActive">Optional filter for active/inactive teachers</param>
    /// <returns>Paginated list of teachers</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedTeacherListDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllTeachersQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            IsActive = isActive
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get a teacher by ID
    /// </summary>
    /// <param name="id">Teacher ID (GUID)</param>
    /// <returns>Teacher details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TeacherDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        var query = new GetTeacherByIdQuery { Id = id };
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
            return NotFound(new { message = $"Teacher with ID {id} not found" });

        return Ok(result);
    }

    /// <summary>
    /// Get a teacher by email
    /// </summary>
    /// <param name="email">Teacher email</param>
    /// <returns>Teacher details</returns>
    [HttpGet("by-email/{email}")]
    [ProducesResponseType(typeof(TeacherDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByEmail(string email, CancellationToken cancellationToken)
    {
        var query = new GetTeacherByEmailQuery { Email = email };
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
            return NotFound(new { message = $"Teacher with email {email} not found" });

        return Ok(result);
    }

    /// <summary>
    /// Create a new teacher
    /// </summary>
    /// <param name="dto">Teacher creation data</param>
    /// <returns>Created teacher details</returns>
    [HttpPost]
    [ProducesResponseType(typeof(TeacherDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateTeacherDto dto,
        CancellationToken cancellationToken)
    {
        var command = new CreateTeacherCommand
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            Qualification = dto.Qualification,
            ExperienceYears = dto.ExperienceYears,
            JoiningDate = dto.JoiningDate,
            CreatedByUserId = GetCurrentUserId()
        };

        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Update a teacher
    /// </summary>
    /// <param name="id">Teacher ID (GUID)</param>
    /// <param name="dto">Teacher update data</param>
    /// <returns>Updated teacher details</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(TeacherDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(
        string id,
        [FromBody] UpdateTeacherDto dto,
        CancellationToken cancellationToken)
    {
        if (id != dto.Id)
            return BadRequest(new { message = "ID in URL and body do not match" });

        var command = new UpdateTeacherCommand
        {
            Id = dto.Id,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            Qualification = dto.Qualification,
            ExperienceYears = dto.ExperienceYears,
            IsActive = dto.IsActive,
            UpdatedByUserId = GetCurrentUserId()
        };

        try
        {
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { message = $"Teacher with ID {id} not found" });
        }
    }

    /// <summary>
    /// Deactivate a teacher
    /// </summary>
    /// <param name="id">Teacher ID (GUID)</param>
    /// <returns>Success status</returns>
    [HttpPatch("{id}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(string id, CancellationToken cancellationToken)
    {
        var command = new DeactivateTeacherCommand
        {
            Id = id,
            UpdatedByUserId = GetCurrentUserId()
        };

        try
        {
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { message = $"Teacher with ID {id} not found" });
        }
    }

    /// <summary>
    /// Activate a teacher
    /// </summary>
    /// <param name="id">Teacher ID (GUID)</param>
    /// <returns>Success status</returns>
    [HttpPatch("{id}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(string id, CancellationToken cancellationToken)
    {
        var command = new ActivateTeacherCommand
        {
            Id = id,
            UpdatedByUserId = GetCurrentUserId()
        };

        try
        {
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { message = $"Teacher with ID {id} not found" });
        }
    }

    /// <summary>
    /// Delete a teacher
    /// </summary>
    /// <param name="id">Teacher ID (GUID)</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        var command = new DeleteTeacherCommand { Id = id };

        try
        {
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { message = $"Teacher with ID {id} not found" });
        }
    }

    /// <summary>
    /// Get active teachers for selection
    /// </summary>
    /// <param name="searchTerm">Optional search term</param>
    /// <returns>List of active teachers</returns>
    [HttpGet("active")]
    [ProducesResponseType(typeof(List<TeacherListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActive(
        [FromQuery] string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetActiveTeachersQuery { SearchTerm = searchTerm };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Check if email exists (for validation)
    /// </summary>
    /// <param name="email">Teacher email</param>
    /// <param name="excludeTeacherId">Optional teacher ID to exclude from check</param>
    /// <returns>Boolean indicating if email exists</returns>
    [HttpGet("check-email/{email}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckEmailExists(
        string email,
        [FromQuery] string? excludeTeacherId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new TeacherEmailExistsQuery
        {
            Email = email,
            ExcludeTeacherId = excludeTeacherId
        };

        var exists = await _mediator.Send(query, cancellationToken);
        return Ok(new { exists });
    }

    /// <summary>
    /// Get assignments for a teacher
    /// </summary>
    /// <param name="id">Teacher ID</param>
    /// <param name="activeOnly">Filter for active assignments only</param>
    /// <returns>List of teacher assignments</returns>
    [HttpGet("{id}/assignments")]
    [ProducesResponseType(typeof(List<TeacherAssignmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAssignments(
        string id,
        [FromQuery] bool? activeOnly = null,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(id, out var teacherId))
            return BadRequest(new { message = "Invalid teacher ID format" });

        var query = new GetTeacherAssignmentsQuery
        {
            TeacherId = teacherId,
            ActiveOnly = activeOnly
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Create a new teacher assignment
    /// </summary>
    /// <param name="id">Teacher ID</param>
    /// <param name="dto">Assignment details</param>
    /// <returns>Created assignment</returns>
    [HttpPost("{id}/assignments")]
    [ProducesResponseType(typeof(TeacherAssignmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateAssignment(
        string id,
        [FromBody] CreateTeacherAssignmentDto dto,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(id, out var teacherId))
            return BadRequest(new { message = "Invalid teacher ID format" });

        var command = new CreateTeacherAssignmentCommand
        {
            TeacherId = teacherId,
            ClassId = dto.ClassId,
            SubjectId = dto.SubjectId,
            AssignmentDate = dto.AssignmentDate
        };

        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetAssignments), new { id = teacherId }, result);
    }

    /// <summary>
    /// Remove a teacher assignment
    /// </summary>
    /// <param name="id">Teacher ID</param>
    /// <param name="assignmentId">Assignment ID</param>
    /// <param name="dto">Removal details</param>
    /// <returns>Success response</returns>
    [HttpDelete("{id}/assignments/{assignmentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveAssignment(
        string id,
        string assignmentId,
        [FromBody] RemoveTeacherAssignmentDto? dto = null,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(assignmentId, out var parsedAssignmentId))
            return BadRequest(new { message = "Invalid assignment ID format" });

        var command = new RemoveTeacherAssignmentCommand
        {
            AssignmentId = parsedAssignmentId,
            RemovalDate = dto?.RemovalDate
        };

        await _mediator.Send(command, cancellationToken);
        return Ok(new { message = "Assignment removed successfully" });
    }

    /// <summary>
    /// Helper method to get current user ID from claims
    /// </summary>
    private string GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("nameid");
        return userIdClaim?.Value ?? Guid.Empty.ToString();
    }

    /// <summary>
    /// Upload teacher profile image
    /// </summary>
    [HttpPost("{id:guid}/upload-image")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadImage(Guid id, IFormFile file, CancellationToken cancellationToken)
    {
        try
        {
            // Verify teacher exists
            var teacher = await _context.Teachers.FindAsync(new object[] { id }, cancellationToken: cancellationToken);
            if (teacher == null)
            {
                return NotFound(new { message = "Teacher not found" });
            }

            // Validate and upload image
            var imagePath = await _imageUploadService.UploadImageAsync(file, "teachers", id, cancellationToken);

            // Delete old image if it exists
            if (!string.IsNullOrEmpty(teacher.ImagePath))
            {
                await _imageUploadService.DeleteImageAsync(teacher.ImagePath, cancellationToken);
            }

            // Update teacher with new image path
            teacher.ImagePath = imagePath;
            _context.Teachers.Update(teacher);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Image uploaded for teacher {TeacherId}: {ImagePath}", id, imagePath);
            return Ok(new { message = "Image uploaded successfully", imagePath });
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Invalid image file for teacher {TeacherId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image for teacher {TeacherId}", id);
            return BadRequest(new { message = "Error uploading image" });
        }
    }
}
