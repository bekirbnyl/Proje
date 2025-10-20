using MediatR;
using Sinema.Application.DTOs.Reservations.Holds;

namespace Sinema.Application.Features.Reservations.Holds.Commands.CreateSeatHolds;

/// <summary>
/// Command to create seat holds for multiple seats
/// </summary>
public class CreateSeatHoldsCommand : IRequest<List<SeatHoldDto>>
{
    /// <summary>
    /// Screening ID for which to create holds
    /// </summary>
    public Guid ScreeningId { get; set; }

    /// <summary>
    /// List of seat IDs to hold
    /// </summary>
    public List<Guid> SeatIds { get; set; } = new();

    /// <summary>
    /// Client token identifying the browser/client
    /// </summary>
    public string ClientToken { get; set; } = string.Empty;

    /// <summary>
    /// Optional user ID if authenticated
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Optional custom TTL in seconds (uses default if not provided)
    /// </summary>
    public int? TtlSeconds { get; set; }
}
