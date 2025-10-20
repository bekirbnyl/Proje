namespace Sinema.Application.DTOs.Reports.Deletions;

/// <summary>
/// Represents a single row of deletions report data
/// </summary>
public class DeletionsReportRow
{
    /// <summary>
    /// Entity name (if grouped by entity)
    /// </summary>
    public string? EntityName { get; set; }

    /// <summary>
    /// Deletion reason (if grouped by reason)
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// User who performed the deletion (if grouped by user)
    /// </summary>
    public string? DeletedBy { get; set; }

    /// <summary>
    /// Number of deletions in this group
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Date of the last deletion in this group
    /// </summary>
    public DateTime LastDeletedAt { get; set; }

    /// <summary>
    /// Date of the first deletion in this group
    /// </summary>
    public DateTime FirstDeletedAt { get; set; }

    /// <summary>
    /// Grouping key for display purposes
    /// </summary>
    public string GroupKey => EntityName ?? Reason ?? DeletedBy ?? "Unknown";
}
