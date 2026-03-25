using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMS.Application.Features.Promotions.Commands;
using SMS.Application.Features.Promotions.DTOs;

namespace SMS.API.Controllers;

[Authorize(Policy = "AdminOnly")]
[ApiController]
[Route("api/v1/[controller]")]
public class PromotionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PromotionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Promote students from one academic year to another in bulk
    /// </summary>
    [HttpPost("bulk")]
    [ProducesResponseType(typeof(PromotionResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PromotionResultDto>> PromoteStudents([FromBody] PromoteStudentsCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }
}
