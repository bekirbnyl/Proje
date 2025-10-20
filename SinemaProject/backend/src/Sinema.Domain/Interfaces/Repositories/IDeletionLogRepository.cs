using Sinema.Domain.Entities;

namespace Sinema.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for DeletionLog entity
/// </summary>
public interface IDeletionLogRepository
{
    /// <summary>
    /// Gets a deletion log by ID
    /// </summary>
    /// <param name="id">Deletion log ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deletion log or null if not found</returns>
    Task<DeletionLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets deletion logs for a specific entity
    /// </summary>
    /// <param name="entityName">Entity type name</param>
    /// <param name="entityId">Entity ID</param>
    /// <param name="limit">Maximum number of entries to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deletion logs ordered by date descending</returns>
    Task<IEnumerable<DeletionLog>> GetByEntityAsync(string entityName, Guid entityId, int limit = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets deletion logs within a date range
    /// </summary>
    /// <param name="startDate">Start date (inclusive)</param>
    /// <param name="endDate">End date (exclusive)</param>
    /// <param name="limit">Maximum number of entries to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deletion logs ordered by date descending</returns>
    Task<IEnumerable<DeletionLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, int limit = 1000, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets deletion logs by user
    /// </summary>
    /// <param name="deletedBy">User ID who performed deletions</param>
    /// <param name="limit">Maximum number of entries to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deletion logs ordered by date descending</returns>
    Task<IEnumerable<DeletionLog>> GetByDeletedByAsync(Guid deletedBy, int limit = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets recent deletion logs
    /// </summary>
    /// <param name="limit">Maximum number of entries to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Recent deletion logs ordered by date descending</returns>
    Task<IEnumerable<DeletionLog>> GetRecentAsync(int limit = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new deletion log entry
    /// </summary>
    /// <param name="deletionLog">Deletion log to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created deletion log</returns>
    Task<DeletionLog> CreateAsync(DeletionLog deletionLog, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new deletion log entry (legacy method name)
    /// </summary>
    /// <param name="deletionLog">Deletion log to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task AddAsync(DeletionLog deletionLog, CancellationToken cancellationToken = default);
}
