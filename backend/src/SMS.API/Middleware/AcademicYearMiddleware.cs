using SMS.Application.Common.Interfaces;

namespace SMS.API.Middleware;

public class AcademicYearMiddleware
{
    private readonly RequestDelegate _next;

    public AcademicYearMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IAcademicYearContext academicYearContext)
    {
        if (context.Request.Headers.TryGetValue("X-Academic-Year-Id", out var yearIdStr))
        {
            if (Guid.TryParse(yearIdStr, out var yearId))
            {
                academicYearContext.AcademicYearId = yearId;
            }
        }

        await _next(context);
    }
}
