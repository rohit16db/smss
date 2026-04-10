using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMS.Application.Features.Notifications.Commands;

namespace SMS.API.Controllers;

/// <summary>
/// General notification endpoints for sending alerts
/// </summary>
[ApiController]
[Route("api/notifications")]
[Authorize] // Any authenticated staff can trigger notifications
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Send a notification using a template
    /// </summary>
    [HttpPost("send")]
    public async Task<IActionResult> SendNotification([FromBody] SendNotificationCommand command)
    {
        var (success, errorMessage) = await _mediator.Send(command);
        
        if (!success)
        {
            return BadRequest(new { success = false, message = errorMessage });
        }

        return Ok(new { success = true, message = "Notification sent successfully" });
    }
}
