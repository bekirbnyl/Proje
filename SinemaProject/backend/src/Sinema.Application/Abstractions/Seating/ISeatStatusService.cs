using Sinema.Application.DTOs.Seating;

namespace Sinema.Application.Abstractions.Seating;

/// <summary>
/// Service for calculating and retrieving seat status for screenings
/// </summary>
public interface ISeatStatusService
{
    /// <summary>
    /// Gets the status of all seats for a specific screening
    /// </summary>
    /// <param name="screeningId">The screening identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of seat status DTOs</returns>
    Task<IEnumerable<SeatStatusDto>> GetSeatStatusesAsync(Guid screeningId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the seat grid response for a specific screening
    /// </summary>
    /// <param name="screeningId">The screening identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Seat grid response with layout ID and seat statuses</returns>
    Task<SeatGridResponse> GetSeatGridAsync(Guid screeningId, CancellationToken cancellationToken = default);
}
