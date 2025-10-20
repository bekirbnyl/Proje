using Sinema.Application.DTOs.Reports.Common;

namespace Sinema.Application.DTOs.Reports.Deletions;

/// <summary>
/// Request DTO for deletions report generation
/// </summary>
public class DeletionsReportRequest : DateRangeRequest
{
    /// <summary>
    /// Grouping criteria (entity, reason, user)
    /// </summary>
    public string By { get; set; } = "entity";

    /// <summary>
    /// Valid grouping options
    /// </summary>
    public static readonly string[] ValidByOptions = { "entity", "reason", "user" };

    /// <summary>
    /// Validates the by parameter
    /// </summary>
    public bool IsValidBy => ValidByOptions.Contains(By, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Normalizes the by parameter to lowercase
    /// </summary>
    public string NormalizedBy => By.ToLowerInvariant();
}
