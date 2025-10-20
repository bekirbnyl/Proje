using MediatR;

namespace Sinema.Application.Features.Halls.Commands.ActivateSeatLayoutVersion;

/// <summary>
/// Command to activate a specific seat layout version for a hall
/// </summary>
public record ActivateSeatLayoutVersionCommand : IRequest<bool>
{
    /// <summary>
    /// Hall identifier
    /// </summary>
    public Guid HallId { get; init; }

    /// <summary>
    /// Seat layout identifier to activate
    /// </summary>
    public Guid LayoutId { get; init; }
}
