namespace Sinema.Domain.Entities;

/// <summary>
/// Represents an audit log entry for settings changes
/// </summary>
public class SettingsChangeLog
{
    /// <summary>
    /// Unique identifier for the change log entry
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The setting key that was changed
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// The old value before the change
    /// </summary>
    public string? OldValue { get; set; }

    /// <summary>
    /// The new value after the change
    /// </summary>
    public string NewValue { get; set; } = string.Empty;

    /// <summary>
    /// ID of the user who made the change
    /// </summary>
    public Guid? ChangedBy { get; set; }

    /// <summary>
    /// When the change was made (UTC)
    /// </summary>
    public DateTime ChangedAt { get; set; }

    /// <summary>
    /// Additional metadata about the change (JSON format)
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// Creates a settings change log entry
    /// </summary>
    /// <param name="key">Setting key</param>
    /// <param name="oldValue">Old value</param>
    /// <param name="newValue">New value</param>
    /// <param name="changedBy">Who made the change</param>
    /// <param name="metadata">Additional metadata</param>
    /// <returns>SettingsChangeLog instance</returns>
    public static SettingsChangeLog Create(string key, string? oldValue, string newValue, Guid? changedBy = null, string? metadata = null)
    {
        return new SettingsChangeLog
        {
            Id = Guid.NewGuid(),
            Key = key,
            OldValue = oldValue,
            NewValue = newValue,
            ChangedBy = changedBy,
            ChangedAt = DateTime.UtcNow,
            Metadata = metadata
        };
    }

    /// <summary>
    /// Checks if this represents a value change (not just a creation)
    /// </summary>
    public bool IsValueChange => OldValue != null;

    /// <summary>
    /// Gets a display string for the change
    /// </summary>
    public string GetChangeDisplay()
    {
        if (OldValue == null)
        {
            return $"Created '{Key}' = '{NewValue}'";
        }
        return $"Changed '{Key}' from '{OldValue}' to '{NewValue}'";
    }
}
