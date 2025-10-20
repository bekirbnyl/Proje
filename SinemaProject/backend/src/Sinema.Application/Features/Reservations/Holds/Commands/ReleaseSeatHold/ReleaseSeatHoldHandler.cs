using MediatR;
using Sinema.Domain.Interfaces.Services;

namespace Sinema.Application.Features.Reservations.Holds.Commands.ReleaseSeatHold;

/// <summary>
/// Handler for ReleaseSeatHoldCommand
/// </summary>
public class ReleaseSeatHoldHandler : IRequestHandler<ReleaseSeatHoldCommand>
{
    private readonly ISeatHoldService _seatHoldService;

    public ReleaseSeatHoldHandler(ISeatHoldService seatHoldService)
    {
        _seatHoldService = seatHoldService;
    }

    public async Task Handle(ReleaseSeatHoldCommand request, CancellationToken cancellationToken)
    {
        await _seatHoldService.ReleaseHoldAsync(
            request.HoldId,
            request.ClientToken,
            request.UserId,
            cancellationToken);
    }
}
