using Sinema.Domain.Entities;

namespace Sinema.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for Hall entities
/// </summary>
public interface IHallRepository
{
    /// <summary>
    /// Gets a hall by its ID
    /// </summary>
    /// <param name="id">Hall ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Hall entity or null if not found</returns>
    Task<Hall?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all halls for a specific cinema
    /// </summary>
    /// <param name="cinemaId">Cinema ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of halls</returns>
    Task<IEnumerable<Hall>> GetByCinemaIdAsync(Guid cinemaId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a hall by cinema and name
    /// </summary>
    /// <param name="cinemaId">Cinema ID</param>
    /// <param name="name">Hall name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Hall entity or null if not found</returns>
    Task<Hall?> GetByNameAsync(Guid cinemaId, string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new hall
    /// </summary>
    /// <param name="hall">Hall entity to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task AddAsync(Hall hall, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing hall
    /// </summary>
    /// <param name="hall">Hall entity to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task UpdateAsync(Hall hall, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a hall
    /// </summary>
    /// <param name="hall">Hall entity to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteAsync(Hall hall, CancellationToken cancellationToken = default);
}
