using Microsoft.Extensions.Logging;
using Quartz;
using Sinema.Domain.Interfaces.Services;

namespace Sinema.Background.Jobs;

/// <summary>
/// Background job that cleans up expired seat holds
/// Runs every minute to remove holds that have exceeded their expiration time
/// </summary>
[DisallowConcurrentExecution]
public class SeatHoldsCleanupJob : IJob
{
    private readonly ISeatHoldService _seatHoldService;
    private readonly ILogger<SeatHoldsCleanupJob> _logger;

    public SeatHoldsCleanupJob(ISeatHoldService seatHoldService, ILogger<SeatHoldsCleanupJob> logger)
    {
        _seatHoldService = seatHoldService;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var startTime = DateTime.UtcNow;
        var totalCleaned = 0;
        var batchSize = 100;

        try
        {
            _logger.LogInformation("Starting seat holds cleanup job at {StartTime}", startTime);

            // Clean in batches to avoid overwhelming the database
            int cleanedInBatch;
            do
            {
                cleanedInBatch = await _seatHoldService.CleanupExpiredHoldsAsync(batchSize, context.CancellationToken);
                totalCleaned += cleanedInBatch;

                // If we cleaned a full batch, there might be more
                if (cleanedInBatch == batchSize)
                {
                    // Small delay to avoid overwhelming the database
                    await Task.Delay(100, context.CancellationToken);
                }
            }
            while (cleanedInBatch == batchSize && !context.CancellationToken.IsCancellationRequested);

            var executionTime = DateTime.UtcNow - startTime;
            
            if (totalCleaned > 0)
            {
                _logger.LogInformation("Seat holds cleanup job completed. Cleaned {TotalCleaned} expired holds in {ExecutionTime:F2}ms",
                    totalCleaned, executionTime.TotalMilliseconds);
            }
            else
            {
                _logger.LogDebug("Seat holds cleanup job completed. No expired holds found. Execution time: {ExecutionTime:F2}ms",
                    executionTime.TotalMilliseconds);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Seat holds cleanup job was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during seat holds cleanup job execution");
            throw;
        }
    }
}
