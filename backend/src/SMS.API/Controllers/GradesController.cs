using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMS.Application.Features.Exams.Commands;
using SMS.Application.Features.Exams.DTOs;
using SMS.Application.Features.Exams.Queries;

namespace SMS.API.Controllers;

/// <summary>
/// Grade Configuration API endpoints
/// Single Responsibility: Handle HTTP requests for grade configuration management
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class GradesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<GradesController> _logger;

    public GradesController(IMediator mediator, ILogger<GradesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    #region Grade Configuration Endpoints

    /// <summary>
    /// Get current grade configuration
    /// GET /api/v1/grades
    /// </summary>
    [HttpGet]
    [AllowAnonymous] // Allow all users to read grades
    [ProducesResponseType(typeof(List<GradeConfigurationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGradeConfiguration()
    {
        try
        {
            var query = new GetGradeConfigurationQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving grade configuration");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error retrieving grade configuration" });
        }
    }

    /// <summary>
    /// Update grade configuration
    /// PUT /api/v1/grades
    /// </summary>
    [HttpPut]
    [ProducesResponseType(typeof(List<GradeConfigurationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateGradeConfiguration([FromBody] UpdateGradeConfigurationDto request)
    {
        try
        {
            var command = new ConfigureGradesCommand { Grades = request.Grades };
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating grade configuration");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Error updating grade configuration" });
        }
    }

    #endregion
}
