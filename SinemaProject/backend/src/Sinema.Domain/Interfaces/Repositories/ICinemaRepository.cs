using Sinema.Domain.Entities;

namespace Sinema.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for Cinema entities
/// </summary>
public interface ICinemaRepository
{
    /// <summary>
    /// Gets a cinema by its ID
    /// </summary>
    /// <param name="id">Cinema ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cinema entity or null if not found</returns>
    Task<Cinema?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all cinemas
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of cinemas</returns>
    Task<IEnumerable<Cinema>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a cinema by name
    /// </summary>
    /// <param name="name">Cinema name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Cinema entity or null if not found</returns>
    Task<Cinema?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new cinema
    /// </summary>
    /// <param name="cinema">Cinema entity to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task AddAsync(Cinema cinema, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing cinema
    /// </summary>
    /// <param name="cinema">Cinema entity to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task UpdateAsync(Cinema cinema, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a cinema
    /// </summary>
    /// <param name="cinema">Cinema entity to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteAsync(Cinema cinema, CancellationToken cancellationToken = default);
}
