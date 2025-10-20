using MediatR;
using Sinema.Application.DTOs.Reports.Deletions;

namespace Sinema.Application.Features.Reports.Deletions;

/// <summary>
/// Query to retrieve deletions report data
/// </summary>
public record GetDeletionsReportQuery : IRequest<DeletionsReportResponse>
{
    /// <summary>
    /// Start date of the report range
    /// </summary>
    public DateTime From { get; init; }

    /// <summary>
    /// End date of the report range
    /// </summary>
    public DateTime To { get; init; }

    /// <summary>
    /// Grouping criteria (entity, reason, user)
    /// </summary>
    public string By { get; init; } = "entity";

    /// <summary>
    /// Creates a query from a request DTO
    /// </summary>
    /// <param name="request">The request DTO</param>
    /// <returns>A new query instance</returns>
    public static GetDeletionsReportQuery FromRequest(DeletionsReportRequest request)
    {
        return new GetDeletionsReportQuery
        {
            From = request.From,
            To = request.To,
            By = request.NormalizedBy
        };
    }
}
