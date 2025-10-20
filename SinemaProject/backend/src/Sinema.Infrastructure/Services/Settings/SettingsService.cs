using Microsoft.EntityFrameworkCore;
using Sinema.Application.Validators.Settings;
using Sinema.Domain.Entities;
using Sinema.Domain.Interfaces.Repositories;
using Sinema.Domain.Interfaces.Services;

namespace Sinema.Infrastructure.Services;

/// <summary>
/// Service implementation for managing application settings with whitelist validation and change tracking
/// </summary>
public class SettingsService : ISettingsService
{
    private readonly ISettingsRepository _settingsRepository;
    private readonly IAuditLogger _auditLogger;

    public SettingsService(ISettingsRepository settingsRepository, IAuditLogger auditLogger)
    {
        _settingsRepository = settingsRepository;
        _auditLogger = auditLogger;
    }

    public async Task<Dictionary<string, string>> GetAsync(string[]? keys = null, CancellationToken cancellationToken = default)
    {
        IEnumerable<Setting> settings;

        if (keys == null || keys.Length == 0)
        {
            // Get all whitelisted settings
            var allSettings = await _settingsRepository.GetAllAsync(cancellationToken);
            settings = allSettings.Where(s => IsKeyWhitelisted(s.Key));
        }
        else
        {
            // Validate all keys are whitelisted
            var invalidKeys = keys.Where(k => !IsKeyWhitelisted(k)).ToArray();
            if (invalidKeys.Length > 0)
            {
                throw new ArgumentException($"Invalid setting keys: {string.Join(", ", invalidKeys)}");
            }

            settings = await _settingsRepository.GetByKeysAsync(keys, cancellationToken);
        }

        return settings.ToDictionary(s => s.Key, s => s.Value);
    }

    public async Task<string?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        if (!IsKeyWhitelisted(key))
        {
            throw new ArgumentException($"Setting key '{key}' is not in the whitelist");
        }

        var setting = await _settingsRepository.GetByKeyAsync(key, cancellationToken);
        return setting?.Value;
    }

    public async Task<Dictionary<string, Setting>> UpdateAsync(Dictionary<string, string> settings, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        if (settings == null || settings.Count == 0)
        {
            return new Dictionary<string, Setting>();
        }

        // Validate all keys and values
        foreach (var kvp in settings)
        {
            var validation = ValidateValue(kvp.Key, kvp.Value);
            if (!validation.IsValid)
            {
                throw new ArgumentException($"Invalid setting '{kvp.Key}': {string.Join(", ", validation.Errors)}");
            }
        }

        var result = new Dictionary<string, Setting>();
        var changeLogEntries = new List<(string Key, string? OldValue, string NewValue)>();

        foreach (var kvp in settings)
        {
            var existing = await _settingsRepository.GetByKeyWithRowVersionAsync(kvp.Key, cancellationToken);
            
            if (existing == null)
            {
                // Create new setting
                var newSetting = Setting.Create(kvp.Key, kvp.Value);
                var created = await _settingsRepository.CreateAsync(newSetting, cancellationToken);
                result[kvp.Key] = created;
                changeLogEntries.Add((kvp.Key, null, kvp.Value));
            }
            else if (existing.Value != kvp.Value)
            {
                // Update existing setting
                var oldValue = existing.Value;
                existing.UpdateValue(kvp.Value);
                var updated = await _settingsRepository.UpdateAsync(existing, cancellationToken);
                result[kvp.Key] = updated;
                changeLogEntries.Add((kvp.Key, oldValue, kvp.Value));
            }
            else
            {
                // No change needed
                result[kvp.Key] = existing;
            }
        }

        // Log all changes
        if (changeLogEntries.Count > 0)
        {
            await _auditLogger.LogSettingsChangesAsync(changeLogEntries, userId, cancellationToken: cancellationToken);
        }

        return result;
    }

    public async Task<Setting> PatchAsync(string key, string value, byte[]? rowVersion = null, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        // Validate key and value
        var validation = ValidateValue(key, value);
        if (!validation.IsValid)
        {
            throw new ArgumentException($"Invalid setting '{key}': {string.Join(", ", validation.Errors)}");
        }

        var existing = await _settingsRepository.GetByKeyWithRowVersionAsync(key, cancellationToken);
        
        if (existing == null)
        {
            // Create new setting
            var newSetting = Setting.Create(key, value);
            var created = await _settingsRepository.CreateAsync(newSetting, cancellationToken);
            
            // Log creation
            await _auditLogger.LogSettingsChangeAsync(key, null, value, userId, cancellationToken: cancellationToken);
            
            return created;
        }

        // Check concurrency control if row version is provided
        if (rowVersion != null && !existing.RowVersion.SequenceEqual(rowVersion))
        {
            throw new DbUpdateConcurrencyException("The setting has been modified by another user. Please refresh and try again.");
        }

        // Update only if value has changed
        if (existing.Value != value)
        {
            var oldValue = existing.Value;
            existing.UpdateValue(value);
            var updated = await _settingsRepository.UpdateAsync(existing, cancellationToken);
            
            // Log change
            await _auditLogger.LogSettingsChangeAsync(key, oldValue, value, userId, cancellationToken: cancellationToken);
            
            return updated;
        }

        return existing;
    }

    public bool IsKeyWhitelisted(string key)
    {
        return WhitelistKeysValidator.IsKeyWhitelisted(key);
    }

    public (bool IsValid, string[] Errors) ValidateValue(string key, string value)
    {
        return WhitelistKeysValidator.ValidateValue(key, value);
    }
}
