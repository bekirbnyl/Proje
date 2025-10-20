using Sinema.Domain.Entities;

namespace Sinema.Domain.Interfaces.Services;

/// <summary>
/// Service for managing application settings with whitelist validation and change tracking
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Gets settings by keys with whitelist filtering
    /// </summary>
    /// <param name="keys">Optional keys to filter. If null, returns all whitelisted settings</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary of key-value pairs</returns>
    Task<Dictionary<string, string>> GetAsync(string[]? keys = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a single setting value by key
    /// </summary>
    /// <param name="key">Setting key</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Setting value or null if not found</returns>
    Task<string?> GetAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates multiple settings with validation and change logging
    /// </summary>
    /// <param name="settings">Settings to update</param>
    /// <param name="userId">ID of the user making the changes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated settings with new row versions</returns>
    Task<Dictionary<string, Setting>> UpdateAsync(Dictionary<string, string> settings, Guid? userId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a single setting with concurrency control
    /// </summary>
    /// <param name="key">Setting key</param>
    /// <param name="value">New value</param>
    /// <param name="rowVersion">Expected row version for concurrency control</param>
    /// <param name="userId">ID of the user making the change</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated setting with new row version</returns>
    Task<Setting> PatchAsync(string key, string value, byte[]? rowVersion = null, Guid? userId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates if a key is in the whitelist
    /// </summary>
    /// <param name="key">Setting key to validate</param>
    /// <returns>True if key is whitelisted</returns>
    bool IsKeyWhitelisted(string key);

    /// <summary>
    /// Validates a setting value according to its type and constraints
    /// </summary>
    /// <param name="key">Setting key</param>
    /// <param name="value">Value to validate</param>
    /// <returns>Validation result with error messages if any</returns>
    (bool IsValid, string[] Errors) ValidateValue(string key, string value);
}
