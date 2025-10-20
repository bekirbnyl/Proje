using Microsoft.EntityFrameworkCore;
using Sinema.Application.Abstractions.Scheduling;
using Sinema.Infrastructure.Persistence;

namespace Sinema.Infrastructure.Services.Scheduling;

/// <summary>
/// Implementation of screening scheduling service
/// </summary>
public class ScreeningSchedulingService : IScreeningSchedulingService
{
    private readonly SinemaDbContext _context;

    public ScreeningSchedulingService(SinemaDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<bool> HasOverlapAsync(Guid hallId, DateTime startAt, int durationMinutes, Guid? excludeScreeningId = null, CancellationToken cancellationToken = default)
    {
        var endAt = startAt.AddMinutes(durationMinutes);

        var query = _context.Screenings
            .Where(s => s.HallId == hallId);

        // Exclude the screening being updated if specified
        if (excludeScreeningId.HasValue)
        {
            query = query.Where(s => s.Id != excludeScreeningId.Value);
        }

        // Check for overlap: StartAt < other.EndAt && EndAt > other.StartAt
        var hasOverlap = await query
            .AnyAsync(s => startAt < s.StartAt.AddMinutes(s.DurationMinutes) && endAt > s.StartAt, cancellationToken);

        return hasOverlap;
    }

    /// <inheritdoc />
    public async Task<bool> IsFirstShowWeekdayAsync(Guid hallId, DateTime startAt, CancellationToken cancellationToken = default)
    {
        // Only check for weekdays (Monday to Friday)
        if (startAt.DayOfWeek == DayOfWeek.Saturday || startAt.DayOfWeek == DayOfWeek.Sunday)
        {
            return false;
        }

        // Get the date part only (ignore time)
        var startDate = startAt.Date;
        var endDate = startDate.AddDays(1);

        // Check if there are any earlier screenings on the same day in the same hall
        var hasEarlierScreening = await _context.Screenings
            .Where(s => s.HallId == hallId)
            .Where(s => s.StartAt >= startDate && s.StartAt < endDate)
            .Where(s => s.StartAt < startAt)
            .AnyAsync(cancellationToken);

        return !hasEarlierScreening;
    }
}
