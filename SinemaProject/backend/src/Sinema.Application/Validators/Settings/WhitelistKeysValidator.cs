using FluentValidation;

namespace Sinema.Application.Validators.Settings;

/// <summary>
/// Validator for settings keys and values based on whitelist and type constraints
/// </summary>
public class WhitelistKeysValidator : AbstractValidator<(string Key, string Value)>
{
    private static readonly Dictionary<string, (Type Type, object? Min, object? Max, string[]? AllowedValues)> WhitelistSettings = new()
    {
        ["HalkGunu"] = (typeof(DayOfWeek), null, null, Enum.GetNames<DayOfWeek>()),
        ["BaseTicketPrice"] = (typeof(decimal), 20m, 500m, null),
        ["SeatHold.DefaultTtlSeconds"] = (typeof(int), 30, 600, null),
        ["Reservation.CloseBeforeStartMinutes"] = (typeof(int), 30, 240, null),
        ["Reservation.ExpireBeforeStartMinutes"] = (typeof(int), 15, 120, null),
        ["Pricing.Student"] = (typeof(decimal), 0m, 1m, null),
        ["Pricing.HalkGunu"] = (typeof(decimal), 0m, 1m, null),
        ["Pricing.FirstWeekday"] = (typeof(decimal), 0m, 1m, null),
        ["Pricing.VipGuest"] = (typeof(decimal), 0m, 1m, null),
        ["Reports.StorageRoot"] = (typeof(string), null, null, null),
        ["VipAdvanceBookingDays"] = (typeof(int), 1, 30, null),
        ["RegularAdvanceBookingDays"] = (typeof(int), 1, 14, null),
        ["MinCreditTopUpAmount"] = (typeof(decimal), 10m, 1000m, null),
        ["ReservationTimeoutMinutes"] = (typeof(int), 5, 60, null),
        ["ReservationCutoffMinutes"] = (typeof(int), 15, 240, null)
    };

    public WhitelistKeysValidator()
    {
        RuleFor(x => x.Key)
            .Must(key => WhitelistSettings.ContainsKey(key))
            .WithMessage(x => $"Setting key '{x.Key}' is not in the whitelist. Allowed keys: {string.Join(", ", WhitelistSettings.Keys)}");

        RuleFor(x => x)
            .Must(x => ValidateValue(x.Key, x.Value).IsValid)
            .WithMessage(x => string.Join("; ", ValidateValue(x.Key, x.Value).Errors));
    }

    /// <summary>
    /// Validates a setting value according to its type and constraints
    /// </summary>
    /// <param name="key">Setting key</param>
    /// <param name="value">Value to validate</param>
    /// <returns>Validation result</returns>
    public static (bool IsValid, string[] Errors) ValidateValue(string key, string value)
    {
        if (!WhitelistSettings.TryGetValue(key, out var config))
        {
            return (false, new[] { $"Setting key '{key}' is not in the whitelist" });
        }

        var errors = new List<string>();

        try
        {
            if (config.Type == typeof(string))
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    errors.Add($"String value cannot be empty for '{key}'");
                }
            }
            else if (config.Type == typeof(int))
            {
                if (!int.TryParse(value, out var intValue))
                {
                    errors.Add($"Value '{value}' is not a valid integer for '{key}'");
                }
                else
                {
                    if (config.Min != null && intValue < (int)config.Min)
                        errors.Add($"Value {intValue} is below minimum {config.Min} for '{key}'");
                    if (config.Max != null && intValue > (int)config.Max)
                        errors.Add($"Value {intValue} is above maximum {config.Max} for '{key}'");
                }
            }
            else if (config.Type == typeof(decimal))
            {
                if (!decimal.TryParse(value, out var decimalValue))
                {
                    errors.Add($"Value '{value}' is not a valid decimal for '{key}'");
                }
                else
                {
                    if (config.Min != null && decimalValue < (decimal)config.Min)
                        errors.Add($"Value {decimalValue} is below minimum {config.Min} for '{key}'");
                    if (config.Max != null && decimalValue > (decimal)config.Max)
                        errors.Add($"Value {decimalValue} is above maximum {config.Max} for '{key}'");
                }
            }
            else if (config.Type == typeof(DayOfWeek))
            {
                if (!Enum.TryParse<DayOfWeek>(value, true, out _))
                {
                    errors.Add($"Value '{value}' is not a valid day of week for '{key}'. Allowed values: {string.Join(", ", config.AllowedValues ?? Array.Empty<string>())}");
                }
            }

            if (config.AllowedValues != null && !config.AllowedValues.Contains(value, StringComparer.OrdinalIgnoreCase))
            {
                errors.Add($"Value '{value}' is not allowed for '{key}'. Allowed values: {string.Join(", ", config.AllowedValues)}");
            }
        }
        catch (Exception ex)
        {
            errors.Add($"Error validating value '{value}' for '{key}': {ex.Message}");
        }

        return (errors.Count == 0, errors.ToArray());
    }

    /// <summary>
    /// Checks if a key is in the whitelist
    /// </summary>
    /// <param name="key">Setting key</param>
    /// <returns>True if whitelisted</returns>
    public static bool IsKeyWhitelisted(string key)
    {
        return WhitelistSettings.ContainsKey(key);
    }

    /// <summary>
    /// Gets all whitelisted keys
    /// </summary>
    /// <returns>Array of whitelisted keys</returns>
    public static string[] GetWhitelistedKeys()
    {
        return WhitelistSettings.Keys.ToArray();
    }
}
