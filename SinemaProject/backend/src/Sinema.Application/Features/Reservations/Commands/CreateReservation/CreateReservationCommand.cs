using MediatR;
using Sinema.Application.DTOs.Reservations;

namespace Sinema.Application.Features.Reservations.Commands.CreateReservation;

/// <summary>
/// Command to create reservations for multiple seats
/// </summary>
public class CreateReservationCommand : IRequest<List<ReservationDto>>
{
    /// <summary>
    /// Screening ID for the reservation
    /// </summary>
    public Guid ScreeningId { get; set; }

    /// <summary>
    /// List of seat IDs to reserve
    /// </summary>
    public List<Guid> SeatIds { get; set; } = new();

    /// <summary>
    /// Client token for hold validation
    /// </summary>
    public string ClientToken { get; set; } = string.Empty;

    /// <summary>
    /// Member ID (if authenticated user)
    /// </summary>
    public Guid? MemberId { get; set; }
}
