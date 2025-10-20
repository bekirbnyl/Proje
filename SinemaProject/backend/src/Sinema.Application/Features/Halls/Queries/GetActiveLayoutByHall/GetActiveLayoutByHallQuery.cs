using MediatR;
using Sinema.Application.DTOs.Seating;

namespace Sinema.Application.Features.Halls.Queries.GetActiveLayoutByHall;

/// <summary>
/// Query to get the active seat layout for a hall
/// </summary>
public record GetActiveLayoutByHallQuery : IRequest<SeatLayoutDto?>
{
    /// <summary>
    /// Hall identifier to get the active layout for
    /// </summary>
    public Guid HallId { get; init; }
}
