using Sinema.Application.DTOs.Reports.Common;

namespace Sinema.Application.DTOs.Reports.Memberships;

/// <summary>
/// Request DTO for memberships report generation
/// </summary>
public class MembershipsReportRequest : DateRangeRequest
{
    /// <summary>
    /// Time granularity for grouping membership data (daily, weekly, monthly)
    /// </summary>
    public string Grain { get; set; } = "daily";

    /// <summary>
    /// Valid grain options
    /// </summary>
    public static readonly string[] ValidGrains = { "daily", "weekly", "monthly" };

    /// <summary>
    /// Validates the grain parameter
    /// </summary>
    public bool IsValidGrain => ValidGrains.Contains(Grain, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Normalizes the grain to lowercase
    /// </summary>
    public string NormalizedGrain => Grain.ToLowerInvariant();
}
