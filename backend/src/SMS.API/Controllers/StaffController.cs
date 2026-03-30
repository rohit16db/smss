using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMS.API.Services;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.StaffManagement.Commands;
using SMS.Application.Features.StaffManagement.DTOs;
using SMS.Application.Features.StaffManagement.Queries;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore; // Added for Include

namespace SMS.API.Controllers;

/// <summary>
/// API controller for Staff management
/// </summary>
[ApiController]
[Route("api/staff")]
[Authorize(Policy = "AcademicAccess")]
public class StaffController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IImageUploadService _imageUploadService;
    private readonly IApplicationDbContext _context;
    private readonly ILogger<StaffController> _logger;

    public StaffController(IMediator mediator, IImageUploadService imageUploadService, IApplicationDbContext context, ILogger<StaffController> logger)
    {
        _mediator = mediator;
        _imageUploadService = imageUploadService;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all staff with pagination
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <param name="searchTerm">Optional search term for name, email, or phone</param>
    /// <param name="isActive">Optional filter for active/inactive staff</param>
    /// <returns>Paginated list of staff</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedStaffListDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllStaffQuery
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
    /// Get a staff member by ID
    /// </summary>
    /// <param name="id">Staff ID (GUID)</param>
    /// <returns>Staff details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(StaffDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        var query = new GetStaffByIdQuery { Id = id };
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
            return NotFound(new { message = $"Staff with ID {id} not found" });

        return Ok(result);
    }

    /// <summary>
    /// Get a staff member by email
    /// </summary>
    /// <param name="email">Staff email</param>
    /// <returns>Staff details</returns>
    [HttpGet("by-email/{email}")]
    [ProducesResponseType(typeof(StaffDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByEmail(string email, CancellationToken cancellationToken)
    {
        var query = new GetStaffByEmailQuery { Email = email };
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
            return NotFound(new { message = $"Staff with email {email} not found" });

        return Ok(result);
    }

    /// <summary>
    /// Create a new staff member
    /// </summary>
    /// <param name="command">Staff creation data</param>
    /// <returns>Created staff details</returns>
    [HttpPost]
    [ProducesResponseType(typeof(StaffDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateStaffCommand command,
        CancellationToken cancellationToken)
    {
        command.CreatedByUserId = GetCurrentUserId();
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Update a staff member
    /// </summary>
    /// <param name="id">Staff ID (GUID)</param>
    /// <param name="command">Staff update data</param>
    /// <returns>Updated staff details</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(StaffDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(
        string id,
        [FromBody] UpdateStaffCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest(new { message = "ID in URL and body do not match" });

        command.UpdatedByUserId = GetCurrentUserId();

        try
        {
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { message = $"Staff with ID {id} not found" });
        }
    }

    /// <summary>
    /// Deactivate a staff member
    /// </summary>
    /// <param name="id">Staff ID (GUID)</param>
    /// <returns>Success status</returns>
    [HttpPatch("{id}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(string id, CancellationToken cancellationToken)
    {
        var command = new DeactivateStaffCommand
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
            return NotFound(new { message = $"Staff with ID {id} not found" });
        }
    }

    /// <summary>
    /// Activate a staff member
    /// </summary>
    /// <param name="id">Staff ID (GUID)</param>
    /// <returns>Success status</returns>
    [HttpPatch("{id}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(string id, CancellationToken cancellationToken)
    {
        var command = new ActivateStaffCommand
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
            return NotFound(new { message = $"Staff with ID {id} not found" });
        }
    }

    /// <summary>
    /// Delete a staff member
    /// </summary>
    /// <param name="id">Staff ID (GUID)</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        var command = new DeleteStaffCommand { Id = id };

        try
        {
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return NotFound(new { message = $"Staff with ID {id} not found" });
        }
    }

    /// <summary>
    /// Get active teachers for selection (filtered by RoleType = Teacher)
    /// </summary>
    /// <param name="searchTerm">Optional search term</param>
    /// <returns>List of active teachers</returns>
    [HttpGet("active")]
    [ProducesResponseType(typeof(List<StaffListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActive(
        [FromQuery] string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetActiveStaffQuery { SearchTerm = searchTerm };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Check if email exists (for validation)
    /// </summary>
    /// <param name="email">Staff email</param>
    /// <param name="excludeStaffId">Optional staff ID to exclude from check</param>
    /// <returns>Boolean indicating if email exists</returns>
    [HttpGet("check-email/{email}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckEmailExists(
        string email,
        [FromQuery] string? excludeStaffId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new StaffEmailExistsQuery
        {
            Email = email,
            ExcludeStaffId = excludeStaffId
        };

        var exists = await _mediator.Send(query, cancellationToken);
        return Ok(new { exists });
    }

    /// <summary>
    /// Get assignments for a teacher
    /// </summary>
    /// <param name="id">Staff ID</param>
    /// <param name="activeOnly">Filter for active assignments only</param>
    /// <returns>List of staff assignments</returns>
    [HttpGet("{id}/assignments")]
    [ProducesResponseType(typeof(List<StaffAssignmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAssignments(
        string id,
        [FromQuery] bool? activeOnly = null,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(id, out var staffId))
            return BadRequest(new { message = "Invalid staff ID format" });

        var query = new GetStaffAssignmentsQuery
        {
            StaffId = staffId,
            ActiveOnly = activeOnly
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get assignments for a section
    /// </summary>
    /// <param name="sectionId">Section ID</param>
    /// <param name="academicYearId">Academic Year ID</param>
    /// <returns>List of staff assignments</returns>
    [HttpGet("section/{sectionId}/{academicYearId}")]
    [ProducesResponseType(typeof(List<StaffAssignmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAssignmentsBySection(
        Guid sectionId,
        Guid academicYearId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetStaffAssignmentsBySectionQuery
        {
            SectionId = sectionId,
            AcademicYearId = academicYearId
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Create a new staff assignment
    /// </summary>
    /// <param name="id">Staff ID</param>
    /// <param name="dto">Assignment details</param>
    /// <returns>Created assignment</returns>
    [HttpPost("{id}/assignments")]
    [ProducesResponseType(typeof(StaffAssignmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateAssignment(
        string id,
        [FromBody] CreateStaffAssignmentDto dto,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(id, out var staffId))
            return BadRequest(new { message = "Invalid staff ID format" });

        var command = new CreateStaffAssignmentCommand
        {
            StaffId = staffId,
            ClassId = dto.ClassId,
            SectionId = dto.SectionId,
            SubjectId = dto.SubjectId,
            AssignmentDate = dto.AssignmentDate
        };

        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetAssignments), new { id = staffId }, result);
    }

    /// <summary>
    /// Remove a staff assignment
    /// </summary>
    /// <param name="id">Staff ID</param>
    /// <param name="assignmentId">Assignment ID</param>
    /// <param name="dto">Removal details</param>
    /// <returns>Success response</returns>
    [HttpDelete("{id}/assignments/{assignmentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveAssignment(
        string id,
        string assignmentId,
        [FromBody] RemoveStaffAssignmentDto? dto = null,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(assignmentId, out var parsedAssignmentId))
            return BadRequest(new { message = "Invalid assignment ID format" });

        var command = new RemoveStaffAssignmentCommand
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
    /// Upload staff profile image
    /// </summary>
    [HttpPost("{id:guid}/upload-image")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadImage(Guid id, IFormFile file, CancellationToken cancellationToken)
    {
        try
        {
            // Verify staff exists along with their profile
            var staff = await _context.Staff
                .Include(s => s.UserProfile)
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

            if (staff == null)
            {
                return NotFound(new { message = "Staff member not found" });
            }

            // Validate and upload image
            var imagePath = await _imageUploadService.UploadImageAsync(file, "staff", id, cancellationToken);

            // Delete old image if it exists
            if (staff.UserProfile != null && !string.IsNullOrEmpty(staff.UserProfile.ImagePath))
            {
                await _imageUploadService.DeleteImageAsync(staff.UserProfile.ImagePath, cancellationToken);
            }

            // Update user profile with new image path
            if (staff.UserProfile != null)
            {
                staff.UserProfile.ImagePath = imagePath;
                _context.UserProfiles.Update(staff.UserProfile);
                await _context.SaveChangesAsync(cancellationToken);
            }

            _logger.LogInformation("Image uploaded for staff {StaffId}: {ImagePath}", id, imagePath);
            return Ok(new { message = "Image uploaded successfully", imagePath });
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Invalid image file for staff {StaffId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image for staff {StaffId}", id);
            return BadRequest(new { message = "Error uploading image" });
        }
    }
}
