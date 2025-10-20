using Sinema.Domain.Entities;

namespace Sinema.Domain.Interfaces.Services;

/// <summary>
/// Service for logging audit events and changes
/// </summary>
public interface IAuditLogger
{
    /// <summary>
    /// Logs a settings change
    /// </summary>
    /// <param name="key">Setting key</param>
    /// <param name="oldValue">Old value</param>
    /// <param name="newValue">New value</param>
    /// <param name="changedBy">ID of the user who made the change</param>
    /// <param name="metadata">Additional metadata</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created change log entry</returns>
    Task<SettingsChangeLog> LogSettingsChangeAsync(string key, string? oldValue, string newValue, Guid? changedBy = null, string? metadata = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs multiple settings changes in a batch
    /// </summary>
    /// <param name="changes">Collection of changes to log</param>
    /// <param name="changedBy">ID of the user who made the changes</param>
    /// <param name="metadata">Additional metadata</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created change log entries</returns>
    Task<IEnumerable<SettingsChangeLog>> LogSettingsChangesAsync(IEnumerable<(string Key, string? OldValue, string NewValue)> changes, Guid? changedBy = null, string? metadata = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs an entity deletion
    /// </summary>
    /// <param name="entityName">Name of the entity type</param>
    /// <param name="entityId">ID of the entity</param>
    /// <param name="reason">Deletion reason</param>
    /// <param name="deletedBy">ID of the user who performed the deletion</param>
    /// <param name="approvedBy">ID of the supervisor who approved the deletion</param>
    /// <param name="metadata">Additional metadata</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created deletion log entry</returns>
    Task<DeletionLog> LogDeletionAsync(string entityName, Guid entityId, string reason, Guid? deletedBy = null, Guid? approvedBy = null, string? metadata = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs an entity restoration
    /// </summary>
    /// <param name="entityName">Name of the entity type</param>
    /// <param name="entityId">ID of the entity</param>
    /// <param name="restoredBy">ID of the user who performed the restoration</param>
    /// <param name="metadata">Additional metadata</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created deletion log entry for restore operation</returns>
    Task<DeletionLog> LogRestoreAsync(string entityName, Guid entityId, Guid? restoredBy = null, string? metadata = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit logs for a specific entity
    /// </summary>
    /// <param name="entityName">Name of the entity type</param>
    /// <param name="entityId">ID of the entity</param>
    /// <param name="limit">Maximum number of entries to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Audit log entries</returns>
    Task<IEnumerable<DeletionLog>> GetEntityAuditLogsAsync(string entityName, Guid entityId, int limit = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets settings change history for a specific key
    /// </summary>
    /// <param name="key">Setting key</param>
    /// <param name="limit">Maximum number of entries to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Settings change log entries</returns>
    Task<IEnumerable<SettingsChangeLog>> GetSettingsHistoryAsync(string key, int limit = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets recent audit activity
    /// </summary>
    /// <param name="limit">Maximum number of entries to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Recent audit activity</returns>
    Task<(IEnumerable<DeletionLog> Deletions, IEnumerable<SettingsChangeLog> SettingsChanges)> GetRecentActivityAsync(int limit = 100, CancellationToken cancellationToken = default);
}
