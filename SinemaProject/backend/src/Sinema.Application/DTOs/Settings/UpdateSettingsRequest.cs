namespace Sinema.Application.DTOs.Settings;

/// <summary>
/// Request DTO for updating multiple settings
/// </summary>
public record UpdateSettingsRequest
{
    /// <summary>
    /// Collection of settings to update
    /// </summary>
    public ICollection<UpdateSettingItem> Items { get; init; } = new List<UpdateSettingItem>();
}

/// <summary>
/// Individual setting item for bulk update
/// </summary>
public record UpdateSettingItem
{
    /// <summary>
    /// Setting key
    /// </summary>
    public string Key { get; init; } = string.Empty;

    /// <summary>
    /// New value for the setting
    /// </summary>
    public string Value { get; init; } = string.Empty;
}
