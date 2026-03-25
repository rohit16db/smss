using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMS.API.Extensions;
using SMS.Application.Features.Settings.Commands;
using SMS.Application.Features.Settings.DTOs;
using SMS.Application.Features.Settings.Queries;

namespace SMS.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class SettingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SettingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get school settings/configuration
    /// </summary>
    [HttpGet("school")]
    public async Task<ActionResult<SchoolDto>> GetSchoolSettings()
    {
        var query = new GetSchoolSettingsQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Update school settings (Admin only)
    /// </summary>
    [HttpPut("school")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<SchoolDto>> UpdateSchoolSettings([FromBody] UpdateSchoolSettingsCommand command)
    {
        command.UpdatedByUserId = User.GetCurrentUsername() ?? "System";
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Get list of academic years
    /// </summary>
    [HttpGet("academic-years")]
    public async Task<ActionResult<List<AcademicYearDto>>> GetAcademicYears()
    {
        var result = await _mediator.Send(new GetAcademicYearsQuery());
        return Ok(result);
    }

    /// <summary>
    /// Get the currently active academic year
    /// </summary>
    [HttpGet("academic-years/active")]
    public async Task<ActionResult<AcademicYearDto>> GetActiveAcademicYear()
    {
        var result = await _mediator.Send(new GetActiveAcademicYearQuery());
        if (result == null) return NotFound("No active academic year found");
        return Ok(result);
    }

    /// <summary>
    /// Create a new academic year (Admin only)
    /// </summary>
    [HttpPost("academic-years")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<AcademicYearDto>> CreateAcademicYear([FromBody] CreateAcademicYearCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetAcademicYears), result);
    }

    /// <summary>
    /// Toggle active status for an academic year (Admin only)
    /// </summary>
    [HttpPatch("academic-years/{id}/toggle-status")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<bool>> ToggleStatus(Guid id)
    {
        var result = await _mediator.Send(new ToggleAcademicYearStatusCommand { Id = id });
        return Ok(result);
    }

    /// <summary>
    /// Upload school logo (Admin only)
    /// </summary>
    [HttpPost("school/logo")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<SchoolDto>> UploadLogo(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file provided");

        if (file.Length > 5 * 1024 * 1024) // 5MB limit
            return BadRequest("File size exceeds 5MB limit");

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var fileExtension = Path.GetExtension(file.FileName).ToLower();

        if (!allowedExtensions.Contains(fileExtension))
            return BadRequest("Invalid file type. Only images allowed.");

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);

        var command = new UpdateSchoolSettingsCommand
        {
            LogoImage = ms.ToArray(),
            LogoFileName = file.FileName
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
