namespace Sinema.Application.DTOs.Reports.Memberships;

/// <summary>
/// Represents a single row of memberships report data
/// </summary>
public class MembershipsReportRow
{
    /// <summary>
    /// Date of the membership data
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Number of membership applications submitted
    /// </summary>
    public int ApplicationCount { get; set; }

    /// <summary>
    /// Number of approved memberships
    /// </summary>
    public int ApprovedCount { get; set; }

    /// <summary>
    /// Number of rejected memberships
    /// </summary>
    public int RejectedCount { get; set; }

    /// <summary>
    /// Number of pending memberships (no decision yet)
    /// </summary>
    public int PendingCount { get; set; }

    /// <summary>
    /// Number of VIP memberships granted
    /// </summary>
    public int VipCount { get; set; }

    /// <summary>
    /// Ratio of VIP memberships to total approved memberships
    /// </summary>
    public decimal VipRatio { get; set; }

    /// <summary>
    /// Approval rate (approved / total processed)
    /// </summary>
    public decimal ApprovalRate { get; set; }

    /// <summary>
    /// Rejection rate (rejected / total processed)
    /// </summary>
    public decimal RejectionRate { get; set; }

    /// <summary>
    /// Total processed applications (approved + rejected)
    /// </summary>
    public int ProcessedCount => ApprovedCount + RejectedCount;

    /// <summary>
    /// Calculates derived metrics
    /// </summary>
    public void CalculateMetrics()
    {
        var totalProcessed = ProcessedCount;
        ApprovalRate = totalProcessed > 0 ? (decimal)ApprovedCount / totalProcessed : 0;
        RejectionRate = totalProcessed > 0 ? (decimal)RejectedCount / totalProcessed : 0;
        VipRatio = ApprovedCount > 0 ? (decimal)VipCount / ApprovedCount : 0;
    }
}
