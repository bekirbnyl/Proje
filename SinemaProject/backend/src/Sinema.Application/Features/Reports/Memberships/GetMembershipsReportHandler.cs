using MediatR;
using Sinema.Application.Abstractions.Reporting;
using Sinema.Application.DTOs.Reports.Memberships;

namespace Sinema.Application.Features.Reports.Memberships;

/// <summary>
/// Handler for processing memberships report queries
/// </summary>
public class GetMembershipsReportHandler : IRequestHandler<GetMembershipsReportQuery, MembershipsReportResponse>
{
    private readonly IReportBuilder<MembershipsReportRequest, MembershipsReportResponse> _membershipsReportBuilder;

    /// <summary>
    /// Initializes a new instance of the GetMembershipsReportHandler
    /// </summary>
    /// <param name="membershipsReportBuilder">The memberships report builder</param>
    public GetMembershipsReportHandler(IReportBuilder<MembershipsReportRequest, MembershipsReportResponse> membershipsReportBuilder)
    {
        _membershipsReportBuilder = membershipsReportBuilder;
    }

    /// <summary>
    /// Handles the memberships report query
    /// </summary>
    /// <param name="request">The query request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The memberships report response</returns>
    public async Task<MembershipsReportResponse> Handle(GetMembershipsReportQuery request, CancellationToken cancellationToken)
    {
        var reportRequest = new MembershipsReportRequest
        {
            From = request.From,
            To = request.To,
            Grain = request.Grain
        };

        return await _membershipsReportBuilder.BuildReportAsync(reportRequest, cancellationToken);
    }
}
