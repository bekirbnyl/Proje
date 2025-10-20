namespace Sinema.Application.Abstractions.Scheduling;

/// <summary>
/// Service for special day handling (Halk Günü, etc.)
/// </summary>
public interface ISpecialDayService
{
    /// <summary>
    /// Checks if the given date falls on Halk Günü (special day)
    /// </summary>
    /// <param name="date">Date to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the date is Halk Günü, false otherwise</returns>
    Task<bool> IsHalkGunuAsync(DateTime date, CancellationToken cancellationToken = default);
}
