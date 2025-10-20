using Sinema.Domain.Entities;

namespace Sinema.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for SeatLayout entities
/// </summary>
public interface ISeatLayoutRepository
{
    /// <summary>
    /// Gets a seat layout by its ID
    /// </summary>
    /// <param name="id">Seat layout ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>SeatLayout entity or null if not found</returns>
    Task<SeatLayout?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the active seat layout for a specific hall
    /// </summary>
    /// <param name="hallId">Hall ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Active SeatLayout or null if not found</returns>
    Task<SeatLayout?> GetActiveByHallIdAsync(Guid hallId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all seat layouts for a specific hall
    /// </summary>
    /// <param name="hallId">Hall ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of seat layouts</returns>
    Task<IEnumerable<SeatLayout>> GetByHallIdAsync(Guid hallId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific version of seat layout for a hall
    /// </summary>
    /// <param name="hallId">Hall ID</param>
    /// <param name="version">Layout version</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>SeatLayout entity or null if not found</returns>
    Task<SeatLayout?> GetByHallAndVersionAsync(Guid hallId, int version, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new seat layout
    /// </summary>
    /// <param name="seatLayout">SeatLayout entity to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task AddAsync(SeatLayout seatLayout, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing seat layout
    /// </summary>
    /// <param name="seatLayout">SeatLayout entity to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task UpdateAsync(SeatLayout seatLayout, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a seat layout
    /// </summary>
    /// <param name="seatLayout">SeatLayout entity to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteAsync(SeatLayout seatLayout, CancellationToken cancellationToken = default);
}
