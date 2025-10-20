namespace Sinema.Application.Abstractions.Reporting;

/// <summary>
/// Interface for building different types of reports
/// </summary>
/// <typeparam name="TRequest">The request type containing report parameters</typeparam>
/// <typeparam name="TResponse">The response type containing report data</typeparam>
public interface IReportBuilder<in TRequest, TResponse>
{
    /// <summary>
    /// Builds a report based on the provided request parameters
    /// </summary>
    /// <param name="request">The request containing report parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The report response containing aggregated data</returns>
    Task<TResponse> BuildReportAsync(TRequest request, CancellationToken cancellationToken = default);
}
