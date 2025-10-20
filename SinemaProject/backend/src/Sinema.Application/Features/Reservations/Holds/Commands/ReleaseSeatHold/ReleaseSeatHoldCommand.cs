using MediatR;

namespace Sinema.Application.Features.Reservations.Holds.Commands.ReleaseSeatHold;

/// <summary>
/// Command to release a seat hold
/// </summary>
public class ReleaseSeatHoldCommand : IRequest
{
    /// <summary>
    /// Hold ID to release
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
