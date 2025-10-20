namespace Sinema.Application.DTOs.Settings;

/// <summary>
/// Request DTO for patching a single setting
/// </summary>
public record PatchSettingRequest
{
    /// <summary>
    /// New value for the setting
    /// </summary>
    public string Value { get; init; } = string.Empty;

    /// <summary>
    /// Row version for optimistic concurrency control (base64 encoded)
    /// Optional - if not provided, concurrency check will be skipped
    /// </summary>
    public string? RowVersion { get; init; }
}
