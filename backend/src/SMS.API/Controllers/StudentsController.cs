using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMS.API.Services;
using SMS.Application.Common.Interfaces;
using SMS.Application.Students.Commands;
using SMS.Application.Students.DTOs;
using SMS.Application.Students.Queries;
using System.ComponentModel.DataAnnotations;

namespace SMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AcademicViewAccess")]
public class StudentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<StudentsController> _logger;
    private readonly IImageUploadService _imageUploadService;
    private readonly IApplicationDbContext _context;

    public StudentsController(IMediator mediator, ILogger<StudentsController> logger, IImageUploadService imageUploadService, IApplicationDbContext context)
    {
        _mediator = mediator;
        _logger = logger;
        _imageUploadService = imageUploadService;
        _context = context;
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
    [Authorize(Policy = "AcademicAccess")]
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
    [Authorize(Policy = "AcademicAccess")]
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
    [Authorize(Policy = "AcademicAccess")]
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
    [Authorize(Policy = "AcademicAccess")]
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
    [Authorize(Policy = "AcademicAccess")]
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

    /// <summary>
    /// Upload student profile image
    /// </summary>
    [HttpPost("{id:guid}/upload-image")]
    [Authorize(Policy = "AcademicAccess")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadImage(Guid id, IFormFile file, CancellationToken cancellationToken)
    {
        try
        {
            // Verify student exists
            var student = await _context.Students.FindAsync(new object[] { id }, cancellationToken: cancellationToken);
            if (student == null)
            {
                return NotFound(new { message = "Student not found" });
            }

            // Validate and upload image
            var imagePath = await _imageUploadService.UploadImageAsync(file, "students", id, cancellationToken);

            // Delete old image if it exists
            if (!string.IsNullOrEmpty(student.ImagePath))
            {
                await _imageUploadService.DeleteImageAsync(student.ImagePath, cancellationToken);
            }

            // Update student with new image path
            student.ImagePath = imagePath;
            student.UpdatedAt = DateTime.UtcNow;
            _context.Students.Update(student);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Image uploaded for student {StudentId}: {ImagePath}", id, imagePath);
            return Ok(new { message = "Image uploaded successfully", imagePath });
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Invalid image file for student {StudentId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image for student {StudentId}", id);
            return BadRequest(new { message = "Error uploading image" });
        }
    }

    /// <summary>
    /// Generate and download student registration form PDF
    /// </summary>
    /// <param name="id">Student ID (GUID)</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <returns>PDF file</returns>
    [HttpGet("{id:guid}/registration-form")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRegistrationFormPdf(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var command = new GenerateStudentRegistrationFormPdfCommand
            {
                StudentId = id
            };

            var pdf = await _mediator.Send(command, cancellationToken);

            return File(pdf, "application/pdf", $"registration-form-{id}.pdf");
        }
        catch (SMS.Domain.Exceptions.EntityNotFoundException ex)
        {
            _logger.LogWarning(ex, "Student not found for registration form PDF generation: {StudentId}", id);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating student registration form PDF for {StudentId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error generating registration form");
        }
    }
}
