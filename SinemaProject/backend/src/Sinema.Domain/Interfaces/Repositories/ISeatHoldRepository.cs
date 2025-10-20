using Sinema.Domain.Entities;

namespace Sinema.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for seat hold operations
/// </summary>
public interface ISeatHoldRepository
{
    /// <summary>
    /// Gets a seat hold by its ID
    /// </summary>
    /// <param name="id">Hold ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Seat hold if found, null otherwise</returns>
    Task<SeatHold?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active (non-expired) holds for specific seats in a screening
    /// </summary>
    /// <param name="screeningId">Screening ID</param>
    /// <param name="seatIds">Collection of seat IDs to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of active holds</returns>
    Task<IEnumerable<SeatHold>> GetActiveHoldsForSeatsAsync(Guid screeningId, IEnumerable<Guid> seatIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active holds for a specific seat in a screening
    /// </summary>
    /// <param name="screeningId">Screening ID</param>
    /// <param name="seatId">Seat ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of active holds for the seat</returns>
    Task<IEnumerable<SeatHold>> GetActiveHoldsBySeatAsync(Guid screeningId, Guid seatId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active holds for a screening
    /// </summary>
    /// <param name="screeningId">Screening ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of active holds</returns>
    Task<IEnumerable<SeatHold>> GetActiveHoldsByScreeningAsync(Guid screeningId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets holds by client token and optional user ID
    /// </summary>
    /// <param name="clientToken">Client token</param>
    /// <param name="userId">Optional user ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of holds</returns>
    Task<IEnumerable<SeatHold>> GetByClientTokenAsync(string clientToken, Guid? userId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets expired holds
    /// </summary>
    /// <param name="currentTime">Current time to compare against</param>
    /// <param name="batchSize">Maximum number of expired holds to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of expired holds</returns>
    Task<IEnumerable<SeatHold>> GetExpiredHoldsAsync(DateTime currentTime, int batchSize = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple seat holds in a single transaction
    /// </summary>
    /// <param name="holds">Collection of holds to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    Task AddRangeAsync(IEnumerable<SeatHold> holds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a seat hold
    /// </summary>
    /// <param name="hold">Hold to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    Task UpdateAsync(SeatHold hold, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a seat hold
    /// </summary>
    /// <param name="hold">Hold to remove</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    Task RemoveAsync(SeatHold hold, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes multiple seat holds
    /// </summary>
    /// <param name="holds">Holds to remove</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    Task RemoveRangeAsync(IEnumerable<SeatHold> holds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes holds by screening and seat IDs (used when creating reservations)
    /// </summary>
    /// <param name="screeningId">Screening ID</param>
    /// <param name="seatIds">Seat IDs</param>
    /// <param name="clientToken">Client token for ownership verification</param>
    /// <param name="userId">Optional user ID for ownership verification</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of holds removed</returns>
    Task<int> RemoveHoldsBySeatsAsync(Guid screeningId, IEnumerable<Guid> seatIds, string clientToken, Guid? userId = null, CancellationToken cancellationToken = default);
}
