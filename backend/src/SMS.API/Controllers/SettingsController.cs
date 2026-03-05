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
