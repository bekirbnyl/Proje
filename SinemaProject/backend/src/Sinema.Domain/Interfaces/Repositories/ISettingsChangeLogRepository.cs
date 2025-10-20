using Sinema.Domain.Entities;

namespace Sinema.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for SettingsChangeLog entity
/// </summary>
public interface ISettingsChangeLogRepository
{
    /// <summary>
    /// Creates a new settings change log entry
    /// </summary>
    /// <param name="changeLog">Change log entry to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created change log entry</returns>
    Task<SettingsChangeLog> CreateAsync(SettingsChangeLog changeLog, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates multiple change log entries
    /// </summary>
    /// <param name="changeLogs">Change log entries to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created change log entries</returns>
    Task<IEnumerable<SettingsChangeLog>> CreateManyAsync(IEnumerable<SettingsChangeLog> changeLogs, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets change log entries for a specific setting key
    /// </summary>
    /// <param name="key">Setting key</param>
    /// <param name="limit">Maximum number of entries to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Change log entries ordered by date descending</returns>
    Task<IEnumerable<SettingsChangeLog>> GetByKeyAsync(string key, int limit = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets change log entries by user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="limit">Maximum number of entries to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Change log entries ordered by date descending</returns>
    Task<IEnumerable<SettingsChangeLog>> GetByUserAsync(Guid userId, int limit = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets change log entries within a date range
    /// </summary>
    /// <param name="fromDate">Start date (inclusive)</param>
    /// <param name="toDate">End date (exclusive)</param>
    /// <param name="limit">Maximum number of entries to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Change log entries ordered by date descending</returns>
    Task<IEnumerable<SettingsChangeLog>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, int limit = 1000, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets recent change log entries
    /// </summary>
    /// <param name="limit">Maximum number of entries to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Recent change log entries ordered by date descending</returns>
    Task<IEnumerable<SettingsChangeLog>> GetRecentAsync(int limit = 100, CancellationToken cancellationToken = default);
}
