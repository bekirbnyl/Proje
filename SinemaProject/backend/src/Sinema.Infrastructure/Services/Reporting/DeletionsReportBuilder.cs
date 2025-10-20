using Microsoft.EntityFrameworkCore;
using Sinema.Application.Abstractions.Reporting;
using Sinema.Application.DTOs.Reports.Deletions;
using Sinema.Infrastructure.Persistence;

namespace Sinema.Infrastructure.Services.Reporting;

/// <summary>
/// Builder for generating deletions reports
/// </summary>
public class DeletionsReportBuilder : IReportBuilder<DeletionsReportRequest, DeletionsReportResponse>
{
    private readonly SinemaDbContext _context;

    /// <summary>
    /// Initializes a new instance of the DeletionsReportBuilder
    /// </summary>
    /// <param name="context">Database context</param>
    public DeletionsReportBuilder(SinemaDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Builds a deletions report based on the provided request parameters
    /// </summary>
    /// <param name="request">The request containing report parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The deletions report response containing aggregated data</returns>
    public async Task<DeletionsReportResponse> BuildReportAsync(DeletionsReportRequest request, CancellationToken cancellationToken = default)
    {
        var response = new DeletionsReportResponse
        {
            From = request.From,
            To = request.To,
            GroupBy = request.By
        };

        // Build the base query
        var query = _context.DeletionLogs
            .AsNoTracking()
            .Where(dl => dl.DeletedAt >= request.From && dl.DeletedAt <= request.To.AddDays(1).AddTicks(-1));

        // Execute query
        var deletionLogs = await query.ToListAsync(cancellationToken);

        // Group by the specified criteria
        var groupedData = request.NormalizedBy switch
        {
            "entity" => GroupByEntity(deletionLogs),
            "reason" => GroupByReason(deletionLogs),
            "user" => GroupByUser(deletionLogs),
            _ => throw new ArgumentException($"Invalid groupBy parameter: {request.By}")
        };

        response.Data = groupedData.ToList();

        // Calculate summary
        response.Summary = CalculateSummary(deletionLogs, response.Data);

        return response;
    }

    /// <summary>
    /// Groups deletion logs by entity type
    /// </summary>
    private IEnumerable<DeletionsReportRow> GroupByEntity(List<Domain.Entities.DeletionLog> deletionLogs)
    {
        return deletionLogs
            .GroupBy(dl => dl.EntityName)
            .Select(g => new DeletionsReportRow
            {
                EntityName = g.Key,
                Count = g.Count(),
                FirstDeletedAt = g.Min(x => x.DeletedAt),
                LastDeletedAt = g.Max(x => x.DeletedAt)
            })
            .OrderByDescending(row => row.Count);
    }

    /// <summary>
    /// Groups deletion logs by reason
    /// </summary>
    private IEnumerable<DeletionsReportRow> GroupByReason(List<Domain.Entities.DeletionLog> deletionLogs)
    {
        return deletionLogs
            .GroupBy(dl => dl.Reason)
            .Select(g => new DeletionsReportRow
            {
                Reason = g.Key,
                Count = g.Count(),
                FirstDeletedAt = g.Min(x => x.DeletedAt),
                LastDeletedAt = g.Max(x => x.DeletedAt)
            })
            .OrderByDescending(row => row.Count);
    }

    /// <summary>
    /// Groups deletion logs by user
    /// </summary>
    private IEnumerable<DeletionsReportRow> GroupByUser(List<Domain.Entities.DeletionLog> deletionLogs)
    {
        return deletionLogs
            .GroupBy(dl => dl.DeletedBy?.ToString() ?? "System")
            .Select(g => new DeletionsReportRow
            {
                DeletedBy = g.Key,
                Count = g.Count(),
                FirstDeletedAt = g.Min(x => x.DeletedAt),
                LastDeletedAt = g.Max(x => x.DeletedAt)
            })
            .OrderByDescending(row => row.Count);
    }

    /// <summary>
    /// Calculates summary statistics for the report
    /// </summary>
    private DeletionsReportSummary CalculateSummary(List<Domain.Entities.DeletionLog> allDeletions, IList<DeletionsReportRow> data)
    {
        if (!allDeletions.Any())
        {
            return new DeletionsReportSummary();
        }

        var uniqueEntities = allDeletions.Select(d => d.EntityName).Distinct().Count();
        var uniqueUsers = allDeletions.Where(d => d.DeletedBy.HasValue)
            .Select(d => d.DeletedBy!.Value).Distinct().Count();

        var mostCommonReason = allDeletions
            .GroupBy(d => d.Reason)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault()?.Key;

        return new DeletionsReportSummary
        {
            TotalDeletions = allDeletions.Count,
            UniqueEntities = uniqueEntities,
            UniqueUsers = uniqueUsers,
            MostCommonReason = mostCommonReason,
            FirstDeletion = allDeletions.Min(d => d.DeletedAt),
            LastDeletion = allDeletions.Max(d => d.DeletedAt)
        };
    }
}
