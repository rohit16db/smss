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
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized(new { message = "Invalid user ID in token" });
            }

            var user = await _mediator.Send(new Application.Features.Users.Queries.GetUserByIdQuery { UserId = parsedUserId });
            
            if (user == null)
            {
                return Unauthorized(new { message = "User not found" });
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user");
            return StatusCode(500, new { message = "An error occurred while retrieving user information" });
        }
    }

    /// <summary>
    /// Change password (requires authentication)
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var parsedUserId))
        {
            return Unauthorized();
        }

        var command = new ChangePasswordCommand
        {
            UserId = parsedUserId,
            CurrentPassword = request.CurrentPassword,
            NewPassword = request.NewPassword
        };

        await _mediator.Send(command);
        _logger.LogInformation("User {UserId} changed password successfully", parsedUserId);
        return Ok(new { message = "Password changed successfully" });
    }

    /// <summary>
    /// Request password reset (public endpoint)
    /// </summary>
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var command = new ForgotPasswordCommand
        {
            Email = request.Email
        };

        await _mediator.Send(command);
        _logger.LogInformation("Password reset requested for email {Email}", request.Email);
        
        // Always return success to prevent email enumeration
        return Ok(new { message = "If the email exists, a password reset link has been sent" });
    }

    /// <summary>
    /// Reset password using reset token (public endpoint)
    /// </summary>
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var command = new ResetPasswordCommand
        {
            Email = request.Email,
            ResetToken = request.ResetToken,
            NewPassword = request.NewPassword
        };

        await _mediator.Send(command);
        _logger.LogInformation("Password reset successfully for email {Email}", request.Email);
        return Ok(new { message = "Password reset successfully" });
    }

    /// <summary>
    /// Logout and revoke refresh token (requires authentication)
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var parsedUserId))
        {
            return Unauthorized();
        }

        var command = new LogoutCommand
        {
            UserId = parsedUserId
        };

        await _mediator.Send(command);
        _logger.LogInformation("User {UserId} logged out successfully", parsedUserId);
        return Ok(new { message = "Logged out successfully" });
    }
}
