using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;
using Sinema.Domain.Enums;
using Sinema.Infrastructure.Persistence;

namespace Sinema.Background.Jobs;

/// <summary>
/// Background job that expires pending reservations based on T-30 rule
/// Runs every 5 minutes to check for reservations that need to be expired
/// </summary>
[DisallowConcurrentExecution]
public class ReservationsExpireJob : IJob
{
    private readonly SinemaDbContext _context;
    private readonly ILogger<ReservationsExpireJob> _logger;

    public ReservationsExpireJob(SinemaDbContext context, ILogger<ReservationsExpireJob> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var startTime = DateTime.UtcNow;
        var totalExpired = 0;

        try
        {
            _logger.LogInformation("Starting reservations expire job at {StartTime}", startTime);

            // Find pending reservations that have expired
            var currentTime = DateTime.UtcNow;
            var expiredReservations = await _context.Reservations
                .Where(r => r.Status == ReservationStatus.Pending && r.ExpiresAt <= currentTime)
                .ToListAsync(context.CancellationToken);

            if (expiredReservations.Any())
            {
                foreach (var reservation in expiredReservations)
                {
                    reservation.MarkAsExpired();
                }

                await _context.SaveChangesAsync(context.CancellationToken);
                totalExpired = expiredReservations.Count;

                var executionTime = DateTime.UtcNow - startTime;
                _logger.LogInformation("Reservations expire job completed. Expired {TotalExpired} reservations in {ExecutionTime:F2}ms",
                    totalExpired, executionTime.TotalMilliseconds);
            }
            else
            {
                var executionTime = DateTime.UtcNow - startTime;
                _logger.LogDebug("Reservations expire job completed. No expired reservations found. Execution time: {ExecutionTime:F2}ms",
                    executionTime.TotalMilliseconds);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Reservations expire job was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during reservations expire job execution");
            throw;
        }
    }
}
