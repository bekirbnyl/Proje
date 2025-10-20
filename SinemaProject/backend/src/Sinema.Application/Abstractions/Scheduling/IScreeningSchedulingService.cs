namespace Sinema.Application.Abstractions.Scheduling;

/// <summary>
/// Service for screening scheduling logic and validations
/// </summary>
public interface IScreeningSchedulingService
{
    /// <summary>
    /// Checks if a screening has overlapping time with existing screenings in the same hall
    /// </summary>
    /// <param name="hallId">Hall identifier</param>
    /// <param name="startAt">Screening start time</param>
    /// <param name="durationMinutes">Screening duration in minutes</param>
    /// <param name="excludeScreeningId">Optional screening ID to exclude from overlap check (for updates)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if there is an overlap, false otherwise</returns>
    Task<bool> HasOverlapAsync(Guid hallId, DateTime startAt, int durationMinutes, Guid? excludeScreeningId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a screening is the first weekday show of the day in the specified hall
    /// </summary>
    /// <param name="hallId">Hall identifier</param>
    /// <param name="startAt">Screening start time</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if this is the first weekday show, false otherwise</returns>
    Task<bool> IsFirstShowWeekdayAsync(Guid hallId, DateTime startAt, CancellationToken cancellationToken = default);
}
