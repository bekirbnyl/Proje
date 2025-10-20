namespace Sinema.Application.Abstractions.Settings;

/// <summary>
/// Service for reading application settings from database or configuration
/// </summary>
public interface IAppSettingsReader
{
    /// <summary>
    /// Gets the Halk Günü (special day) setting value
    /// </summary>
    /// <returns>Halk Günü setting value (e.g., "Wednesday")</returns>
    Task<string?> GetHalkGunuAsync();

    /// <summary>
    /// Gets the base ticket price setting
    /// </summary>
    /// <returns>Base ticket price or null if not set</returns>
    Task<decimal?> GetBaseTicketPriceAsync();

    /// <summary>
    /// Gets a setting value by key
    /// </summary>
    /// <param name="key">Setting key</param>
    /// <returns>Setting value or null if not found</returns>
    Task<string?> GetSettingAsync(string key);

    /// <summary>
    /// Gets a setting value as decimal
    /// </summary>
    /// <param name="key">Setting key</param>
    /// <returns>Decimal value or null if not found or invalid</returns>
    Task<decimal?> GetDecimalSettingAsync(string key);

    /// <summary>
    /// Gets a setting value as boolean
    /// </summary>
    /// <param name="key">Setting key</param>
    /// <returns>Boolean value or null if not found or invalid</returns>
    Task<bool?> GetBoolSettingAsync(string key);

    /// <summary>
    /// Gets a setting value as integer
    /// </summary>
    /// <param name="key">Setting key</param>
    /// <returns>Integer value or null if not found or invalid</returns>
    Task<int?> GetIntSettingAsync(string key);
}
