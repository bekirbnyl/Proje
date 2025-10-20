using Sinema.Application.DTOs.Reports.Common;

namespace Sinema.Application.DTOs.Reports.Sales;

/// <summary>
/// Request DTO for sales report generation
/// </summary>
public class SalesReportRequest : DateRangeRequest
{
    /// <summary>
    /// Time granularity for grouping sales data (daily, weekly, monthly)
    /// </summary>
    public string Grain { get; set; } = "daily";

    /// <summary>
    /// Grouping criteria (film, hall, screening)
    /// </summary>
    public string By { get; set; } = "film";

    /// <summary>
    /// Sales channel filter (web, boxoffice, mobile, all)
    /// </summary>
    public string Channel { get; set; } = "all";

    /// <summary>
    /// Valid grain options
    /// </summary>
    public static readonly string[] ValidGrains = { "daily", "weekly", "monthly" };

    /// <summary>
    /// Valid grouping options
    /// </summary>
    public static readonly string[] ValidByOptions = { "film", "hall", "screening" };

    /// <summary>
    /// Valid channel options
    /// </summary>
    public static readonly string[] ValidChannels = { "web", "boxoffice", "mobile", "all" };

    /// <summary>
    /// Validates the grain parameter
    /// </summary>
    public bool IsValidGrain => ValidGrains.Contains(Grain, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Validates the by parameter
    /// </summary>
    public bool IsValidBy => ValidByOptions.Contains(By, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Validates the channel parameter
    /// </summary>
    public bool IsValidChannel => ValidChannels.Contains(Channel, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Normalizes the grain to lowercase
    /// </summary>
    public string NormalizedGrain => Grain.ToLowerInvariant();

    /// <summary>
    /// Normalizes the by parameter to lowercase
    /// </summary>
    public string NormalizedBy => By.ToLowerInvariant();

    /// <summary>
    /// Normalizes the channel parameter to lowercase
    /// </summary>
    public string NormalizedChannel => Channel.ToLowerInvariant();
}
