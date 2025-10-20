using MediatR;
using Sinema.Application.DTOs.Seating;

namespace Sinema.Application.Features.Halls.Commands.AddSeatLayoutVersion;

/// <summary>
/// Command to add a new seat layout version to a hall
/// </summary>
public record AddSeatLayoutVersionCommand : IRequest<SeatLayoutDto>
{
    /// <summary>
    /// Hall identifier to add the layout version to
    /// </summary>
    public Guid HallId { get; init; }

    /// <summary>
    /// Collection of seats to be created in the new layout version
    /// </summary>
    public IEnumerable<CreateSeatDto> Seats { get; init; } = Enumerable.Empty<CreateSeatDto>();
}
