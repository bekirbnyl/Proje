using MediatR;
using Sinema.Application.Abstractions.Reporting;
using Sinema.Application.DTOs.Reports.Deletions;

namespace Sinema.Application.Features.Reports.Deletions;

/// <summary>
/// Handler for processing deletions report queries
/// </summary>
public class GetDeletionsReportHandler : IRequestHandler<GetDeletionsReportQuery, DeletionsReportResponse>
{
    private readonly IReportBuilder<DeletionsReportRequest, DeletionsReportResponse> _deletionsReportBuilder;

    /// <summary>
    /// Initializes a new instance of the GetDeletionsReportHandler
    /// </summary>
    /// <param name="deletionsReportBuilder">The deletions report builder</param>
    public GetDeletionsReportHandler(IReportBuilder<DeletionsReportRequest, DeletionsReportResponse> deletionsReportBuilder)
    {
        _deletionsReportBuilder = deletionsReportBuilder;
    }

    /// <summary>
    /// Handles the deletions report query
    /// </summary>
    /// <param name="request">The query request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The deletions report response</returns>
    public async Task<DeletionsReportResponse> Handle(GetDeletionsReportQuery request, CancellationToken cancellationToken)
    {
        var reportRequest = new DeletionsReportRequest
        {
            From = request.From,
            To = request.To,
            By = request.By
        };

        return await _deletionsReportBuilder.BuildReportAsync(reportRequest, cancellationToken);
    }
}
