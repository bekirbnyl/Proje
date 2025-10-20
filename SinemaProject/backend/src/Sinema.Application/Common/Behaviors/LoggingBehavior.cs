using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Sinema.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior for request/response logging and performance monitoring
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var requestId = Guid.NewGuid();

        _logger.LogInformation("Handling request {RequestName} with ID {RequestId}",
            requestName, requestId);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();

            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            if (elapsedMs > 500) // Log slow requests
            {
                _logger.LogWarning("Slow request {RequestName} with ID {RequestId} took {ElapsedMs}ms",
                    requestName, requestId, elapsedMs);
            }
            else
            {
                _logger.LogInformation("Completed request {RequestName} with ID {RequestId} in {ElapsedMs}ms",
                    requestName, requestId, elapsedMs);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            _logger.LogError(ex, "Request {RequestName} with ID {RequestId} failed after {ElapsedMs}ms",
                requestName, requestId, elapsedMs);

            throw;
        }
    }
}
