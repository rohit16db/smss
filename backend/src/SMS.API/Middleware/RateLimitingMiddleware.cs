using System.Collections.Concurrent;
using System.Net;

namespace SMS.API.Middleware
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RateLimitingMiddleware> _logger;
        private readonly ConcurrentDictionary<string, ClientRequest> _clients;
        private readonly int _requestsPerMinute;

        public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
            _clients = new ConcurrentDictionary<string, ClientRequest>();
            _requestsPerMinute = 60; // Default: 60 requests per minute
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.Request.Path.Value ?? "unknown";
            var clientId = GetClientIdentifier(context);

            // Skip rate limiting for health checks and swagger
            if (endpoint.Contains("/health") || endpoint.Contains("/swagger"))
            {
                await _next(context);
                return;
            }

            if (IsRateLimited(clientId))
            {
                _logger.LogWarning($"Rate limit exceeded for client {clientId} on endpoint {endpoint}");
                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                context.Response.ContentType = "application/json";
                
                var errorResponse = new { 
                    code = "RATE_LIMIT_EXCEEDED", 
                    message = "Too many requests. Please try again later.",
                    retryAfter = 60
                };
                
                await context.Response.WriteAsJsonAsync(errorResponse);
                return;
            }

            await _next(context);
        }

        private string GetClientIdentifier(HttpContext context)
        {
            // Prefer X-Forwarded-For header (behind proxy), fallback to RemoteIpAddress
            var forwarded = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwarded))
            {
                return forwarded.Split(',').First().Trim();
            }

            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        private bool IsRateLimited(string clientId)
        {
            var now = DateTime.UtcNow;

            var clientRequest = _clients.AddOrUpdate(clientId,
                new ClientRequest { FirstRequest = now, RequestCount = 1 },
                (key, existing) =>
                {
                    var timeSpan = now - existing.FirstRequest;

                    if (timeSpan.TotalMinutes >= 1)
                    {
                        // Reset if more than 1 minute has passed
                        return new ClientRequest { FirstRequest = now, RequestCount = 1 };
                    }

                    existing.RequestCount++;
                    return existing;
                });

            // Return true if exceeded limit
            return clientRequest.RequestCount > _requestsPerMinute;
        }

        private class ClientRequest
        {
            public DateTime FirstRequest { get; set; }
            public int RequestCount { get; set; }
        }
    }
}
