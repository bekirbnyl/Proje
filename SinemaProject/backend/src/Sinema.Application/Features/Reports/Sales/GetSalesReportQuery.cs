using MediatR;
using Sinema.Application.DTOs.Reports.Sales;

namespace Sinema.Application.Features.Reports.Sales;

/// <summary>
/// Query to retrieve sales report data
/// </summary>
public record GetSalesReportQuery : IRequest<SalesReportResponse>
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
    /// Time granularity for grouping (daily, weekly, monthly)
    /// </summary>
    public string Grain { get; init; } = "daily";

    /// <summary>
    /// Grouping criteria (film, hall, screening)
    /// </summary>
    public string By { get; init; } = "film";

    /// <summary>
    /// Sales channel filter (web, boxoffice, mobile, all)
    /// </summary>
    public string Channel { get; init; } = "all";

    /// <summary>
    /// Creates a query from a request DTO
    /// </summary>
    /// <param name="request">The request DTO</param>
    /// <returns>A new query instance</returns>
    public static GetSalesReportQuery FromRequest(SalesReportRequest request)
    {
        return new GetSalesReportQuery
        {
            From = request.From,
            To = request.To,
            Grain = request.NormalizedGrain,
            By = request.NormalizedBy,
            Channel = request.NormalizedChannel
        };
    }
}
