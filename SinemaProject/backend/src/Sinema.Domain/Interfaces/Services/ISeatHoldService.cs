using Sinema.Domain.Entities;

namespace Sinema.Domain.Interfaces.Services;

/// <summary>
/// Service interface for seat hold business logic
/// </summary>
public interface ISeatHoldService
{
    /// <summary>
    /// Creates holds for multiple seats in a screening
    /// </summary>
    /// <param name="screeningId">Screening ID</param>
    /// <param name="seatIds">Collection of seat IDs to hold</param>
    /// <param name="clientToken">Client token for ownership</param>
    /// <param name="userId">Optional user ID</param>
    /// <param name="ttlSeconds">Time-to-live in seconds (optional, uses default if not provided)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of created holds</returns>
    Task<IEnumerable<SeatHold>> CreateHoldsAsync(
        Guid screeningId, 
        IEnumerable<Guid> seatIds, 
        string clientToken, 
        Guid? userId = null, 
        int? ttlSeconds = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Extends the expiration time of a hold (heartbeat mechanism)
    /// </summary>
    /// <param name="holdId">Hold ID</param>
    /// <param name="clientToken">Client token for ownership verification</param>
    /// <param name="userId">Optional user ID for ownership verification</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated hold with new expiration time</returns>
    Task<SeatHold> ExtendHoldAsync(Guid holdId, string clientToken, Guid? userId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Releases a hold manually
    /// </summary>
    /// <param name="holdId">Hold ID</param>
    /// <param name="clientToken">Client token for ownership verification</param>
    /// <param name="userId">Optional user ID for ownership verification</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    Task ReleaseHoldAsync(Guid holdId, string clientToken, Guid? userId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Releases holds by reservation ID (called after reservation confirm/cancel)
    /// </summary>
    /// <param name="reservationId">Reservation ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of holds released</returns>
    Task<int> ReleaseHoldsByReservationAsync(Guid reservationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that specified seats are held by the provided client/user for reservation creation
    /// </summary>
    /// <param name="screeningId">Screening ID</param>
    /// <param name="seatIds">Seat IDs to validate</param>
    /// <param name="clientToken">Client token</param>
    /// <param name="userId">Optional user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if all seats are properly held by the client</returns>
    Task<bool> ValidateHoldsForReservationAsync(
        Guid screeningId, 
        IEnumerable<Guid> seatIds, 
        string clientToken, 
        Guid? userId = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cleans up expired holds
    /// </summary>
    /// <param name="batchSize">Maximum number of holds to clean in one batch</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of holds cleaned up</returns>
    Task<int> CleanupExpiredHoldsAsync(int batchSize = 100, CancellationToken cancellationToken = default);
}
