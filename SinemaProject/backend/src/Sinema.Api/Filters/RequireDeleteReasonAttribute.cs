using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Sinema.Application.Validators.Common;

namespace Sinema.Api.Filters;

/// <summary>
/// Attribute to require and validate deletion reason for DELETE operations
/// </summary>
public class RequireDeleteReasonAttribute : ActionFilterAttribute
{
    /// <summary>
    /// Parameter name to look for the reason (default: "reason")
    /// </summary>
    public string ParameterName { get; set; } = "reason";

    /// <summary>
    /// Whether to allow reason from request body as well as query parameter
    /// </summary>
    public bool AllowFromBody { get; set; } = false;

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        // Only apply to DELETE requests
        if (!string.Equals(context.HttpContext.Request.Method, "DELETE", StringComparison.OrdinalIgnoreCase))
        {
            base.OnActionExecuting(context);
            return;
        }

        string? reason = null;

        // Try to get reason from query parameter
        if (context.HttpContext.Request.Query.TryGetValue(ParameterName, out var reasonQuery))
        {
            reason = reasonQuery.FirstOrDefault();
        }

        // Try to get reason from action parameters if not found in query
        if (string.IsNullOrWhiteSpace(reason) && context.ActionArguments.TryGetValue(ParameterName, out var reasonParam))
        {
            reason = reasonParam?.ToString();
        }

        // Try to get reason from request body if allowed
        if (string.IsNullOrWhiteSpace(reason) && AllowFromBody)
        {
            // This would require reading the request body, which is more complex
            // For now, we'll stick to query parameters and action parameters
        }

        // Validate reason
        if (string.IsNullOrWhiteSpace(reason))
        {
            context.Result = new BadRequestObjectResult(new
            {
                message = "Deletion reason is required",
                parameter = ParameterName,
                details = "Provide a reason for the deletion using the 'reason' query parameter"
            });
            return;
        }

        // Validate reason content
        var validation = DeleteReasonValidator.ValidateReason(reason);
        if (!validation.IsValid)
        {
            context.Result = new BadRequestObjectResult(new
            {
                message = "Invalid deletion reason",
                errors = validation.Errors
            });
            return;
        }

        // Sanitize and store reason in HTTP context for use by interceptor
        var sanitizedReason = DeleteReasonValidator.SanitizeReason(reason);
        context.HttpContext.Items["DeleteReason"] = sanitizedReason;

        base.OnActionExecuting(context);
    }
}
