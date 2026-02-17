using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;

namespace SMS.API.Controllers;

/// <summary>
/// Health check controller for monitoring API status
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;
    private readonly IApplicationDbContext _context;

    public HealthController(ILogger<HealthController> logger, IApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    /// <summary>
    /// Get application health status
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<HealthStatusDto>> Get()
    {
        _logger.LogInformation("Health check requested");

        var status = new HealthStatusDto
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0"
        };

        try
        {
            // Check database connectivity
            await _context.Users.CountAsync();
            status.Database = "Connected";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            status.Status = "Unhealthy";
            status.Database = "Disconnected";
        }

        return Ok(status);
    }

    /// <summary>
    /// Get detailed system information
    /// </summary>
    [HttpGet("info")]
    [AllowAnonymous]
    public ActionResult<SystemInfoDto> GetInfo()
    {
        return Ok(new SystemInfoDto
        {
            ApplicationName = "School Management Software",
            Version = "1.0.0",
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
            Timestamp = DateTime.UtcNow,
            RuntimeVersion = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription
        });
    }
}

public class HealthStatusDto
{
    public string Status { get; set; } = "Healthy";
    public string Database { get; set; } = "Unknown";
    public DateTime Timestamp { get; set; }
    public string Version { get; set; } = string.Empty;
}

public class SystemInfoDto
{
    public string ApplicationName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string RuntimeVersion { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
