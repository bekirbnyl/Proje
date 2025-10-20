using MediatR;
using Sinema.Application.DTOs.Seating;

namespace Sinema.Application.Features.Halls.Queries.GetSeatLayoutsByHall;

/// <summary>
/// Query to get all seat layout versions for a hall
/// </summary>
public record GetSeatLayoutsByHallQuery : IRequest<IEnumerable<SeatLayoutDto>>
{
    /// <summary>
    /// Hall identifier to get layouts for
    /// </summary>
    public Guid HallId { get; init; }
}
