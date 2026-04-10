using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMS.Application.Features.Notifications.DTOs;
using SMS.Application.Features.Notifications.Handlers;

namespace SMS.API.Controllers;

/// <summary>
/// Manage notification templates and settings
/// </summary>
[ApiController]
[Route("api/settings/notifications")]
[Authorize(Policy = "AdminOnly")]
public class NotificationSettingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationSettingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("templates")]
    public async Task<IActionResult> GetTemplates([FromQuery] string? category)
    {
        var result = await _mediator.Send(new GetNotificationTemplatesQuery { Category = category });
        return Ok(result);
    }

    [HttpGet("templates/{id}")]
    public async Task<IActionResult> GetTemplate(Guid id)
    {
        var result = await _mediator.Send(new GetNotificationTemplateByIdQuery { Id = id });
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost("templates")]
    public async Task<IActionResult> CreateTemplate([FromBody] CreateNotificationTemplateDto dto)
    {
        var result = await _mediator.Send(new CreateNotificationTemplateCommand { Dto = dto });
        return CreatedAtAction(nameof(GetTemplate), new { id = result.Id }, result);
    }

    [HttpPut("templates/{id}")]
    public async Task<IActionResult> UpdateTemplate(Guid id, [FromBody] UpdateNotificationTemplateDto dto)
    {
        if (id != dto.Id) return BadRequest("ID mismatch");
        var result = await _mediator.Send(new UpdateNotificationTemplateCommand { Dto = dto });
        return Ok(result);
    }

    [HttpDelete("templates/{id}")]
    public async Task<IActionResult> DeleteTemplate(Guid id)
    {
        var result = await _mediator.Send(new DeleteNotificationTemplateCommand { Id = id });
        if (!result) return NotFound();
        return NoContent();
    }
}
