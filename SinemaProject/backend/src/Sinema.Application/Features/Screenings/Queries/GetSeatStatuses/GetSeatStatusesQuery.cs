using MediatR;
using Sinema.Application.DTOs.Seating;

namespace Sinema.Application.Features.Screenings.Queries.GetSeatStatuses;

/// <summary>
/// Query to get seat statuses for a specific screening
/// </summary>
public record GetSeatStatusesQuery : IRequest<SeatGridResponse>
{
    /// <summary>
    /// Screening identifier to get seat statuses for
    /// </summary>
    public Guid ScreeningId { get; init; }
}
