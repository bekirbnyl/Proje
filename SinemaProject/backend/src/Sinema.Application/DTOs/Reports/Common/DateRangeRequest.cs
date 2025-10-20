namespace Sinema.Application.DTOs.Reports.Common;

/// <summary>
/// Base class for report requests that require a date range
/// </summary>
public abstract class DateRangeRequest
{
    /// <summary>
    /// Start date of the report range (inclusive)
    /// </summary>
    public DateTime From { get; set; }

    /// <summary>
    /// End date of the report range (inclusive)
    /// </summary>
    public DateTime To { get; set; }

    /// <summary>
    /// Output format for the report (json or xlsx)
    /// </summary>
    public string Format { get; set; } = "json";

    /// <summary>
    /// Validates the date range
    /// </summary>
    public bool IsValidDateRange => From <= To && (To - From).Days <= 366;

    /// <summary>
    /// Checks if the output format is Excel
    /// </summary>
    public bool IsExcelFormat => Format.Equals("xlsx", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Checks if the output format is JSON
    /// </summary>
    public bool IsJsonFormat => Format.Equals("json", StringComparison.OrdinalIgnoreCase);
}
