using Sinema.Domain.Entities;
using Sinema.Domain.Interfaces.Repositories;
using Sinema.Domain.Interfaces.Services;

namespace Sinema.Infrastructure.Services;

/// <summary>
/// Service implementation for logging audit events and changes
/// </summary>
public class AuditLogger : IAuditLogger
{
    private readonly ISettingsChangeLogRepository _settingsChangeLogRepository;
    private readonly IDeletionLogRepository _deletionLogRepository;

    public AuditLogger(
        ISettingsChangeLogRepository settingsChangeLogRepository,
        IDeletionLogRepository deletionLogRepository)
    {
        _settingsChangeLogRepository = settingsChangeLogRepository;
        _deletionLogRepository = deletionLogRepository;
    }

    public async Task<SettingsChangeLog> LogSettingsChangeAsync(string key, string? oldValue, string newValue, Guid? changedBy = null, string? metadata = null, CancellationToken cancellationToken = default)
    {
        var changeLog = SettingsChangeLog.Create(key, oldValue, newValue, changedBy, metadata);
        return await _settingsChangeLogRepository.CreateAsync(changeLog, cancellationToken);
    }

    public async Task<IEnumerable<SettingsChangeLog>> LogSettingsChangesAsync(IEnumerable<(string Key, string? OldValue, string NewValue)> changes, Guid? changedBy = null, string? metadata = null, CancellationToken cancellationToken = default)
    {
        var changeLogs = changes.Select(change => 
            SettingsChangeLog.Create(change.Key, change.OldValue, change.NewValue, changedBy, metadata));

        return await _settingsChangeLogRepository.CreateManyAsync(changeLogs, cancellationToken);
    }

    public async Task<DeletionLog> LogDeletionAsync(string entityName, Guid entityId, string reason, Guid? deletedBy = null, Guid? approvedBy = null, string? metadata = null, CancellationToken cancellationToken = default)
    {
        var deletionLog = DeletionLog.Create(entityName, entityId, reason, deletedBy, approvedBy);
        deletionLog.Metadata = metadata;

        return await _deletionLogRepository.CreateAsync(deletionLog, cancellationToken);
    }

    public async Task<DeletionLog> LogRestoreAsync(string entityName, Guid entityId, Guid? restoredBy = null, string? metadata = null, CancellationToken cancellationToken = default)
    {
        var restoreLog = DeletionLog.Create(entityName, entityId, "RESTORED", restoredBy);
        restoreLog.Metadata = metadata;

        return await _deletionLogRepository.CreateAsync(restoreLog, cancellationToken);
    }

    public async Task<IEnumerable<DeletionLog>> GetEntityAuditLogsAsync(string entityName, Guid entityId, int limit = 50, CancellationToken cancellationToken = default)
    {
        return await _deletionLogRepository.GetByEntityAsync(entityName, entityId, limit, cancellationToken);
    }

    public async Task<IEnumerable<SettingsChangeLog>> GetSettingsHistoryAsync(string key, int limit = 50, CancellationToken cancellationToken = default)
    {
        return await _settingsChangeLogRepository.GetByKeyAsync(key, limit, cancellationToken);
    }

    public async Task<(IEnumerable<DeletionLog> Deletions, IEnumerable<SettingsChangeLog> SettingsChanges)> GetRecentActivityAsync(int limit = 100, CancellationToken cancellationToken = default)
    {
        var deletions = await _deletionLogRepository.GetRecentAsync(limit / 2, cancellationToken);
        var settingsChanges = await _settingsChangeLogRepository.GetRecentAsync(limit / 2, cancellationToken);

        return (deletions, settingsChanges);
    }
}
