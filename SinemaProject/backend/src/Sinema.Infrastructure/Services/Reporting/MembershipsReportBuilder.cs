using Microsoft.EntityFrameworkCore;
using Sinema.Application.Abstractions.Reporting;
using Sinema.Application.DTOs.Reports.Memberships;
using Sinema.Infrastructure.Persistence;

namespace Sinema.Infrastructure.Services.Reporting;

/// <summary>
/// Builder for generating memberships reports
/// </summary>
public class MembershipsReportBuilder : IReportBuilder<MembershipsReportRequest, MembershipsReportResponse>
{
    private readonly SinemaDbContext _context;

    /// <summary>
    /// Initializes a new instance of the MembershipsReportBuilder
    /// </summary>
    /// <param name="context">Database context</param>
    public MembershipsReportBuilder(SinemaDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Builds a memberships report based on the provided request parameters
    /// </summary>
    /// <param name="request">The request containing report parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The memberships report response containing aggregated data</returns>
    public async Task<MembershipsReportResponse> BuildReportAsync(MembershipsReportRequest request, CancellationToken cancellationToken = default)
    {
        var response = new MembershipsReportResponse
        {
            From = request.From,
            To = request.To,
            Grain = request.Grain
        };

        // Get member applications in the date range
        var membersQuery = _context.Members
            .AsNoTracking()
            .Include(m => m.Approvals)
            .Where(m => m.CreatedAt >= request.From && m.CreatedAt <= request.To.AddDays(1).AddTicks(-1));

        var members = await membersQuery.ToListAsync(cancellationToken);

        // Get approval data that might have happened after member creation but in the date range
        var approvalsQuery = _context.MemberApprovals
            .AsNoTracking()
            .Include(ma => ma.Member)
            .Where(ma => ma.CreatedAt >= request.From && ma.CreatedAt <= request.To.AddDays(1).AddTicks(-1));

        var approvals = await approvalsQuery.ToListAsync(cancellationToken);

        // Group data by time grain
        var groupedData = GroupByGrain(members, approvals, request.NormalizedGrain);

        response.Data = groupedData.ToList();

        // Calculate summary
        response.Summary = CalculateSummary(members, approvals, request);

        return response;
    }

    /// <summary>
    /// Groups membership data by the specified time grain
    /// </summary>
    private IEnumerable<MembershipsReportRow> GroupByGrain(
        List<Domain.Entities.Member> members,
        List<Domain.Entities.MemberApproval> approvals,
        string grain)
    {
        // Get all unique dates in the range
        var memberDates = members.Select(m => GetDateGrouping(m.CreatedAt, grain));
        var approvalDates = approvals.Select(a => GetDateGrouping(a.CreatedAt, grain));
        var allDates = memberDates.Concat(approvalDates).Distinct().OrderBy(d => d);

        return allDates.Select(date => CreateMembershipsReportRow(date, members, approvals, grain));
    }

    /// <summary>
    /// Creates a memberships report row for a specific date
    /// </summary>
    private MembershipsReportRow CreateMembershipsReportRow(
        DateTime date,
        List<Domain.Entities.Member> allMembers,
        List<Domain.Entities.MemberApproval> allApprovals,
        string grain)
    {
        // Filter data for this specific date grouping
        var membersForDate = allMembers
            .Where(m => GetDateGrouping(m.CreatedAt, grain) == date)
            .ToList();

        var approvalsForDate = allApprovals
            .Where(a => GetDateGrouping(a.CreatedAt, grain) == date)
            .ToList();

        // Count applications (new member registrations)
        var applicationCount = membersForDate.Count;

        // Count approvals and rejections
        var approvedCount = approvalsForDate.Count(a => a.Approved);
        var rejectedCount = approvalsForDate.Count(a => !a.Approved);

        // Count pending (members without any approval decision)
        var pendingCount = membersForDate.Count(m => !m.Approvals.Any());

        // Count VIP members (those with VIP status among approved)
        var vipCount = membersForDate.Count(m => m.VipStatus && m.Approvals.Any(a => a.Approved));

        var row = new MembershipsReportRow
        {
            Date = date,
            ApplicationCount = applicationCount,
            ApprovedCount = approvedCount,
            RejectedCount = rejectedCount,
            PendingCount = pendingCount,
            VipCount = vipCount
        };

        row.CalculateMetrics();
        return row;
    }

    /// <summary>
    /// Groups date by the specified grain
    /// </summary>
    private DateTime GetDateGrouping(DateTime date, string grain)
    {
        return grain switch
        {
            "daily" => date.Date,
            "weekly" => date.Date.AddDays(-(int)date.DayOfWeek), // Start of week (Sunday)
            "monthly" => new DateTime(date.Year, date.Month, 1),
            _ => date.Date
        };
    }

    /// <summary>
    /// Calculates summary statistics for the report
    /// </summary>
    private MembershipsReportSummary CalculateSummary(
        List<Domain.Entities.Member> members,
        List<Domain.Entities.MemberApproval> approvals,
        MembershipsReportRequest request)
    {
        if (!members.Any() && !approvals.Any())
        {
            return new MembershipsReportSummary();
        }

        var totalApplications = members.Count;
        var totalApproved = approvals.Count(a => a.Approved);
        var totalRejected = approvals.Count(a => !a.Approved);
        var totalPending = members.Count(m => !m.Approvals.Any());
        var totalVip = members.Count(m => m.VipStatus && m.Approvals.Any(a => a.Approved));

        var totalProcessed = totalApproved + totalRejected;
        var overallApprovalRate = totalProcessed > 0 ? (decimal)totalApproved / totalProcessed : 0;
        var overallRejectionRate = totalProcessed > 0 ? (decimal)totalRejected / totalProcessed : 0;
        var overallVipRatio = totalApproved > 0 ? (decimal)totalVip / totalApproved : 0;

        var daysDiff = (request.To - request.From).Days + 1;
        var avgApplicationsPerDay = daysDiff > 0 ? (decimal)totalApplications / daysDiff : 0;

        return new MembershipsReportSummary
        {
            TotalApplications = totalApplications,
            TotalApproved = totalApproved,
            TotalRejected = totalRejected,
            TotalPending = totalPending,
            TotalVip = totalVip,
            OverallApprovalRate = overallApprovalRate,
            OverallRejectionRate = overallRejectionRate,
            OverallVipRatio = overallVipRatio,
            AvgApplicationsPerDay = avgApplicationsPerDay
        };
    }
}
