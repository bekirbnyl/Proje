namespace Sinema.Application.DTOs.Reports.Memberships;

/// <summary>
/// Response DTO for memberships report containing aggregated data
/// </summary>
public class MembershipsReportResponse
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
    /// Granularity used for grouping
    /// </summary>
    public string Grain { get; set; } = string.Empty;

    /// <summary>
    /// Collection of membership data rows
    /// </summary>
    public IList<MembershipsReportRow> Data { get; set; } = new List<MembershipsReportRow>();

    /// <summary>
    /// Summary statistics for the entire report period
    /// </summary>
    public MembershipsReportSummary Summary { get; set; } = new();

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
/// Summary statistics for the memberships report
/// </summary>
public class MembershipsReportSummary
{
    /// <summary>
    /// Total membership applications in the period
    /// </summary>
    public int TotalApplications { get; set; }

    /// <summary>
    /// Total approved memberships
    /// </summary>
    public int TotalApproved { get; set; }

    /// <summary>
    /// Total rejected memberships
    /// </summary>
    public int TotalRejected { get; set; }

    /// <summary>
    /// Total pending memberships
    /// </summary>
    public int TotalPending { get; set; }

    /// <summary>
    /// Total VIP memberships granted
    /// </summary>
    public int TotalVip { get; set; }

    /// <summary>
    /// Overall approval rate
    /// </summary>
    public decimal OverallApprovalRate { get; set; }

    /// <summary>
    /// Overall rejection rate
    /// </summary>
    public decimal OverallRejectionRate { get; set; }

    /// <summary>
    /// Overall VIP ratio
    /// </summary>
    public decimal OverallVipRatio { get; set; }

    /// <summary>
    /// Average applications per day
    /// </summary>
    public decimal AvgApplicationsPerDay { get; set; }
}
