using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMS.Application.Auth.Commands;
using SMS.Application.Auth.DTOs;

namespace SMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IMediator mediator, ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        var command = new RegisterCommand
        {
            Username = request.Username,
            Email = request.Email,
            Password = request.Password,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = request.Role
        };

        var response = await _mediator.Send(command);
        _logger.LogInformation("User {Username} registered successfully", request.Username);
        return Ok(response);
    }

    /// <summary>
    /// Login with username and password
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var command = new LoginCommand
        {
            Username = request.Username,
            Password = request.Password
        };

        var response = await _mediator.Send(command);
        _logger.LogInformation("User {Username} logged in successfully", request.Username);
        return Ok(response);
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var command = new RefreshTokenCommand
        {
            AccessToken = request.AccessToken,
            RefreshToken = request.RefreshToken
        };

        var response = await _mediator.Send(command);
        _logger.LogInformation("Token refreshed successfully");
        return Ok(response);
    }

    /// <summary>
    /// Get current user info (requires authentication)
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<UserDto> GetCurrentUser()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var username = User.Identity?.Name;
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        var roleString = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        var firstName = User.FindFirst("FirstName")?.Value;
        var lastName = User.FindFirst("LastName")?.Value;

        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var parsedUserId))
        {
            return Unauthorized();
        }

        // Parse role safely
        Domain.Enums.UserRole role = Domain.Enums.UserRole.Clerk;
        if (!string.IsNullOrEmpty(roleString))
        {
            if (int.TryParse(roleString, out var roleValue))
            {
                role = (Domain.Enums.UserRole)roleValue;
            }
            else if (Enum.TryParse<Domain.Enums.UserRole>(roleString, true, out var parsedRole))
            {
                role = parsedRole;
            }
        }

        var userDto = new UserDto
        {
            Id = parsedUserId,
            Username = username ?? "",
            Email = email ?? "",
            FirstName = firstName ?? "",
            LastName = lastName ?? "",
            Role = role,
            IsActive = true
        };

        return Ok(userDto);
    }
}
