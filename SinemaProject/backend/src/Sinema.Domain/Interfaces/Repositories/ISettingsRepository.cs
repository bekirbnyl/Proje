using Sinema.Domain.Entities;

namespace Sinema.Domain.Interfaces.Repositories;

/// <summary>
/// Repository interface for Settings entity
/// </summary>
public interface ISettingsRepository
{
    /// <summary>
    /// Gets all settings
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of settings</returns>
    Task<IEnumerable<Setting>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets settings by keys
    /// </summary>
    /// <param name="keys">Keys to filter by</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of settings</returns>
    Task<IEnumerable<Setting>> GetByKeysAsync(string[] keys, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a setting by key
    /// </summary>
    /// <param name="key">Setting key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Setting or null if not found</returns>
    Task<Setting?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a setting by key with row version for concurrency control
    /// </summary>
    /// <param name="key">Setting key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Setting or null if not found</returns>
    Task<Setting?> GetByKeyWithRowVersionAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new setting
    /// </summary>
    /// <param name="setting">Setting to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created setting</returns>
    Task<Setting> CreateAsync(Setting setting, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing setting
    /// </summary>
    /// <param name="setting">Setting to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated setting</returns>
    Task<Setting> UpdateAsync(Setting setting, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates multiple settings in a transaction
    /// </summary>
    /// <param name="settings">Settings to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated settings</returns>
    Task<IEnumerable<Setting>> UpdateManyAsync(IEnumerable<Setting> settings, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a setting by key
    /// </summary>
    /// <param name="key">Setting key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted, false if not found</returns>
    Task<bool> DeleteAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a setting exists by key
    /// </summary>
    /// <param name="key">Setting key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if exists</returns>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
}