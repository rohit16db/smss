using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using System.Text.Json;
using FluentValidation;

namespace SMS.API.Middleware;

/// <summary>
/// Global exception handling middleware
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var response = new ErrorResponse();

        switch (exception)
        {
            case ValidationException validationEx:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = "Validation failed";
                response.Code = "VALIDATION_ERROR";
                response.Details = string.Join("; ", validationEx.Errors.Select(e => e.ErrorMessage));
                break;

            case InvalidOperationException:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response.Message = exception.Message;
                response.Code = "INVALID_OPERATION";
                break;

            case UnauthorizedAccessException:
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                response.Message = "Unauthorized access";
                response.Code = "UNAUTHORIZED";
                break;

            case KeyNotFoundException:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                response.Message = "Resource not found";
                response.Code = "NOT_FOUND";
                break;

            default:
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                response.Message = exception.Message ?? "An internal server error occurred";
                response.Code = "INTERNAL_ERROR";
                response.Details = exception.InnerException?.Message;
                break;
        }

        response.Timestamp = DateTime.UtcNow;
        return context.Response.WriteAsJsonAsync(response);
    }
}

public class ErrorResponse
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? Details { get; set; }
}
