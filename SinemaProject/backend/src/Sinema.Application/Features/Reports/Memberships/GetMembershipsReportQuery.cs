using MediatR;
using Sinema.Application.DTOs.Reports.Memberships;

namespace Sinema.Application.Features.Reports.Memberships;

/// <summary>
/// Query to retrieve memberships report data
/// </summary>
public record GetMembershipsReportQuery : IRequest<MembershipsReportResponse>
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
    /// Creates a query from a request DTO
    /// </summary>
    /// <param name="request">The request DTO</param>
    /// <returns>A new query instance</returns>
    public static GetMembershipsReportQuery FromRequest(MembershipsReportRequest request)
    {
        return new GetMembershipsReportQuery
        {
            From = request.From,
            To = request.To,
            Grain = request.NormalizedGrain
        };
    }
}
