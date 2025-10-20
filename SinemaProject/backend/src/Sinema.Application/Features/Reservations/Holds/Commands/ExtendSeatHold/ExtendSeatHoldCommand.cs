using MediatR;
using Sinema.Application.DTOs.Reservations.Holds;

namespace Sinema.Application.Features.Reservations.Holds.Commands.ExtendSeatHold;

/// <summary>
/// Command to extend a seat hold's expiration time
/// </summary>
public class ExtendSeatHoldCommand : IRequest<SeatHoldDto>
{
    /// <summary>
    /// Hold ID to extend
    /// </summary>
    public Guid HoldId { get; set; }

    /// <summary>
    /// Client token for ownership verification
    /// </summary>
    public string ClientToken { get; set; } = string.Empty;

    /// <summary>
    /// Optional user ID for ownership verification
    /// </summary>
    public Guid? UserId { get; set; }
}
