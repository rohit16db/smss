using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMS.API.Extensions;
using SMS.Application.Features.Transport.Commands;
using SMS.Application.Features.Transport.DTOs;
using SMS.Application.Features.Transport.Queries;

namespace SMS.API.Controllers;

/// <summary>
/// Transport Management API endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // Standard authorization for now, can be refined to TransportAccess policy
public class TransportController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TransportController> _logger;

    public TransportController(IMediator mediator, ILogger<TransportController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all vehicles
    /// </summary>
    [HttpGet("vehicles")]
    [ProducesResponseType(typeof(List<VehicleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVehicles([FromQuery] bool? isActive = null)
    {
        try
        {
            var query = new GetVehiclesQuery { IsActive = isActive };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vehicles");
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    /// <summary>
    /// Create a new vehicle
    /// </summary>
    [HttpPost("vehicles")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateVehicle([FromBody] AddVehicleCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating vehicle");
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    /// <summary>
    /// Update an existing vehicle
    /// </summary>
    [HttpPut("vehicles/{id}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateVehicle(Guid id, [FromBody] UpdateVehicleCommand command)
    {
        if (id != command.Id) return BadRequest("ID mismatch");
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating vehicle");
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    /// <summary>
    /// Delete a vehicle
    /// </summary>
    [HttpDelete("vehicles/{id}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteVehicle(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new DeleteVehicleCommand { Id = id });
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting vehicle");
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    /// <summary>
    /// Get all transport routes
    /// </summary>
    [HttpGet("routes")]
    [ProducesResponseType(typeof(List<TransportRouteDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoutes([FromQuery] bool? isActive = null)
    {
        try
        {
            var query = new GetRoutesQuery { IsActive = isActive };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transport routes");
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    /// <summary>
    /// Create a new transport route
    /// </summary>
    [HttpPost("routes")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateRoute([FromBody] AddRouteCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating transport route");
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    /// <summary>
    /// Update an existing transport route
    /// </summary>
    [HttpPut("routes/{id}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateRoute(Guid id, [FromBody] UpdateRouteCommand command)
    {
        if (id != command.Id) return BadRequest("ID mismatch");
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating transport route");
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    /// <summary>
    /// Delete a transport route
    /// </summary>
    [HttpDelete("routes/{id}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteRoute(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new DeleteRouteCommand { Id = id });
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting transport route");
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    /// <summary>
    /// Assign a student to a transport route
    /// </summary>
    [HttpPost("assign")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AssignStudentToTransport([FromBody] AssignStudentToTransportCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning student to transport");
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpGet("assignments")]
    [ProducesResponseType(typeof(List<StudentTransportAssignmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAssignments([FromQuery] bool activeOnly = true)
    {
        try
        {
            var result = await _mediator.Send(new GetTransportAssignmentsQuery { ActiveOnly = activeOnly });
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching student transport assignments");
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpDelete("assignments/{id}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeactivateAssignment(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new DeactivateAssignmentCommand { Id = id });
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating student transport assignment");
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpGet("student/{enrollmentId}")]
    [ProducesResponseType(typeof(StudentTransportAssignmentDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStudentTransportStatus(Guid enrollmentId)
    {
        try
        {
            var result = await _mediator.Send(new GetStudentTransportStatusQuery { Id = enrollmentId });
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching student transport status");
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    /// <summary>
    /// Sync transport fees for a student or all students
    /// </summary>
    [HttpPost("sync-fees")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> SyncTransportFees([FromBody] SyncTransportFeeCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing transport fees");
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }
}
