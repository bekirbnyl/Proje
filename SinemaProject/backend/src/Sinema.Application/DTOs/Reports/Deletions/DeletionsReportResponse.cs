namespace Sinema.Application.DTOs.Reports.Deletions;

/// <summary>
/// Response DTO for deletions report containing aggregated data
/// </summary>
public class DeletionsReportResponse
{
    /// <summary>
    /// Start date of the report range
    /// </summary>
    public DateTime From { get; set; }

    /// <summary>
    /// End date of the report range
    /// </summary>
    public DateTime To { get; set; }

    /// <summary>
    /// Grouping criteria used
    /// </summary>
    public string GroupBy { get; set; } = string.Empty;

    /// <summary>
    /// Collection of deletion data rows
    /// </summary>
    public IList<DeletionsReportRow> Data { get; set; } = new List<DeletionsReportRow>();

    /// <summary>
    /// Summary statistics for the entire report period
    /// </summary>
    public DeletionsReportSummary Summary { get; set; } = new();

    /// <summary>
    /// When the report was generated
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Total number of data rows
    /// </summary>
    public int TotalRows => Data.Count;
}

/// <summary>
/// Summary statistics for the deletions report
/// </summary>
public class DeletionsReportSummary
{
    /// <summary>
    /// Total number of deletions in the period
    /// </summary>
    public int TotalDeletions { get; set; }

    /// <summary>
    /// Number of unique entities deleted
    /// </summary>
    public int UniqueEntities { get; set; }

    /// <summary>
    /// Number of unique users who performed deletions
    /// </summary>
    public int UniqueUsers { get; set; }

    /// <summary>
    /// Most common deletion reason
    /// </summary>
    public string? MostCommonReason { get; set; }

    /// <summary>
    /// Date of the first deletion in the period
    /// </summary>
    public DateTime? FirstDeletion { get; set; }

    /// <summary>
    /// Date of the last deletion in the period
    /// </summary>
    public DateTime? LastDeletion { get; set; }
}
