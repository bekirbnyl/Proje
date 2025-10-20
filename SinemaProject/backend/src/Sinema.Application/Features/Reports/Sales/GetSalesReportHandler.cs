using MediatR;
using Sinema.Application.Abstractions.Reporting;
using Sinema.Application.DTOs.Reports.Sales;

namespace Sinema.Application.Features.Reports.Sales;

/// <summary>
/// Handler for processing sales report queries
/// </summary>
public class GetSalesReportHandler : IRequestHandler<GetSalesReportQuery, SalesReportResponse>
{
    private readonly IReportBuilder<SalesReportRequest, SalesReportResponse> _salesReportBuilder;

    /// <summary>
    /// Initializes a new instance of the GetSalesReportHandler
    /// </summary>
    /// <param name="salesReportBuilder">The sales report builder</param>
    public GetSalesReportHandler(IReportBuilder<SalesReportRequest, SalesReportResponse> salesReportBuilder)
    {
        _salesReportBuilder = salesReportBuilder;
    }

    /// <summary>
    /// Handles the sales report query
    /// </summary>
    /// <param name="request">The query request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The sales report response</returns>
    public async Task<SalesReportResponse> Handle(GetSalesReportQuery request, CancellationToken cancellationToken)
    {
        var reportRequest = new SalesReportRequest
        {
            From = request.From,
            To = request.To,
            Grain = request.Grain,
            By = request.By,
            Channel = request.Channel
        };

        return await _salesReportBuilder.BuildReportAsync(reportRequest, cancellationToken);
    }
}
