namespace Sinema.Application.DTOs.Settings;

/// <summary>
/// DTO for representing a setting
/// </summary>
public record SettingDto
{
    /// <summary>
    /// Setting key
    /// </summary>
    public string Key { get; init; } = string.Empty;

    /// <summary>
    /// Setting value
    /// </summary>
    public string Value { get; init; } = string.Empty;

    /// <summary>
    /// Setting description (optional)
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Row version for concurrency control (base64 encoded)
    /// </summary>
    public string? RowVersion { get; init; }

    /// <summary>
    /// When the setting was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; init; }
}
