using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SMS.API.Filters
{
    /// <summary>
    /// Automatic model state validation filter
    /// Returns 400 Bad Request with validation errors if model is invalid
    /// </summary>
    public class ModelStateValidationFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                    );

                var response = new
                {
                    code = "VALIDATION_ERROR",
                    message = "One or more validation errors occurred.",
                    errors = errors,
                    timestamp = DateTime.UtcNow
                };

                context.Result = new BadRequestObjectResult(response);
            }

            base.OnActionExecuting(context);
        }
    }
}
