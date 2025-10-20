using MediatR;
using Sinema.Application.DTOs.Reservations.Holds;
using Sinema.Domain.Interfaces.Services;
using Sinema.Domain.Interfaces.Repositories;

namespace Sinema.Application.Features.Reservations.Holds.Commands.ExtendSeatHold;

/// <summary>
/// Handler for ExtendSeatHoldCommand
/// </summary>
public class ExtendSeatHoldHandler : IRequestHandler<ExtendSeatHoldCommand, SeatHoldDto>
{
    private readonly ISeatHoldService _seatHoldService;
    private readonly ISeatRepository _seatRepository;

    public ExtendSeatHoldHandler(ISeatHoldService seatHoldService, ISeatRepository seatRepository)
    {
        _seatHoldService = seatHoldService;
        _seatRepository = seatRepository;
    }

    public async Task<SeatHoldDto> Handle(ExtendSeatHoldCommand request, CancellationToken cancellationToken)
    {
        var hold = await _seatHoldService.ExtendHoldAsync(
            request.HoldId,
            request.ClientToken,
            request.UserId,
            cancellationToken);

        // Load seat information for response
        var seat = await _seatRepository.GetByIdAsync(hold.SeatId, cancellationToken);

        return new SeatHoldDto
        {
            HoldId = hold.Id,
            SeatId = hold.SeatId,
            ScreeningId = hold.ScreeningId,
            ExpiresAt = hold.ExpiresAt,
            ClientToken = hold.ClientToken,
            CreatedAt = hold.CreatedAt,
            LastHeartbeatAt = hold.LastHeartbeatAt,
            Seat = seat != null ? new HoldSeatInfo
            {
                Row = seat.Row,
                Col = seat.Col,
                Label = seat.Label
            } : null
        };
    }
}
