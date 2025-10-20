using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace Sinema.Api.Middlewares;

/// <summary>
/// Global exception handling middleware that converts exceptions to RFC7807 ProblemDetails
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unhandled exception occurred");
            await HandleExceptionAsync(context, exception);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var problemDetails = CreateProblemDetails(context, exception);

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = problemDetails.Status ?? (int)HttpStatusCode.InternalServerError;

        var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }

    private static ProblemDetails CreateProblemDetails(HttpContext context, Exception exception)
    {
        var (statusCode, title, detail) = exception switch
        {
            ArgumentNullException => (HttpStatusCode.BadRequest, "Bad Request", "A required parameter was null"),
            ArgumentException ex => (HttpStatusCode.BadRequest, "Bad Request", ex.Message),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized", "Access denied"),
            NotImplementedException ex => (HttpStatusCode.NotImplemented, "Not Implemented", ex.Message),
            TimeoutException ex => (HttpStatusCode.RequestTimeout, "Request Timeout", ex.Message),
            _ => (HttpStatusCode.InternalServerError, "Internal Server Error", $"An error occurred: {exception.Message}")
        };

        var problemDetails = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path,
            Type = $"https://httpstatuses.com/{(int)statusCode}"
        };

        // Add exception details in development
        var environment = context.RequestServices.GetService<IWebHostEnvironment>();
        if (environment?.IsDevelopment() == true)
        {
            problemDetails.Extensions["exception"] = exception.ToString();
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
        }

        return problemDetails;
    }
}
