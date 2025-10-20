namespace Sinema.Domain.Entities;

/// <summary>
/// Represents a system configuration setting
/// </summary>
public class Setting
{
    /// <summary>
    /// Unique identifier for the setting
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Unique key for the setting
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Value of the setting
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Description of what this setting controls
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// When the setting was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the setting was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Row version for optimistic concurrency control
    /// </summary>
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Creates a new setting
    /// </summary>
    /// <param name="key">Setting key</param>
    /// <param name="value">Setting value</param>
    /// <param name="description">Optional description</param>
    /// <returns>Setting instance</returns>
    public static Setting Create(string key, string value, string? description = null)
    {
        return new Setting
        {
            Id = Guid.NewGuid(),
            Key = key,
            Value = value,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Updates the setting value
    /// </summary>
    /// <param name="newValue">New value</param>
    public void UpdateValue(string newValue)
    {
        Value = newValue;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the value as a boolean
    /// </summary>
    public bool GetBoolValue() => bool.TryParse(Value, out var result) && result;

    /// <summary>
    /// Gets the value as an integer
    /// </summary>
    public int GetIntValue() => int.TryParse(Value, out var result) ? result : 0;

    /// <summary>
    /// Gets the value as a decimal
    /// </summary>
    public decimal GetDecimalValue() => decimal.TryParse(Value, out var result) ? result : 0m;

    /// <summary>
    /// Common setting keys
    /// </summary>
    public static class Keys
    {
        public const string HalkGunu = "HalkGunu";
        public const string ReservationTimeoutMinutes = "ReservationTimeoutMinutes";
        public const string ReservationCutoffMinutes = "ReservationCutoffMinutes";
        public const string VipAdvanceBookingDays = "VipAdvanceBookingDays";
        public const string RegularAdvanceBookingDays = "RegularAdvanceBookingDays";
        public const string MinCreditTopUpAmount = "MinCreditTopUpAmount";
    }
}
