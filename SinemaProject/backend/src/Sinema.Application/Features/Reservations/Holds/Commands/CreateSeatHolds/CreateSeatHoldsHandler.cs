using MediatR;
using Sinema.Application.DTOs.Reservations.Holds;
using Sinema.Domain.Interfaces.Services;
using Sinema.Domain.Interfaces.Repositories;

namespace Sinema.Application.Features.Reservations.Holds.Commands.CreateSeatHolds;

/// <summary>
/// Handler for CreateSeatHoldsCommand
/// </summary>
public class CreateSeatHoldsHandler : IRequestHandler<CreateSeatHoldsCommand, List<SeatHoldDto>>
{
    private readonly ISeatHoldService _seatHoldService;
    private readonly ISeatRepository _seatRepository;

    public CreateSeatHoldsHandler(ISeatHoldService seatHoldService, ISeatRepository seatRepository)
    {
        _seatHoldService = seatHoldService;
        _seatRepository = seatRepository;
    }

    public async Task<List<SeatHoldDto>> Handle(CreateSeatHoldsCommand request, CancellationToken cancellationToken)
    {
        var holds = await _seatHoldService.CreateHoldsAsync(
            request.ScreeningId,
            request.SeatIds,
            request.ClientToken,
            request.UserId,
            request.TtlSeconds,
            cancellationToken);

        // Convert to DTOs
        var holdsDtoTasks = holds.Select(async hold =>
        {
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
        });

        return (await Task.WhenAll(holdsDtoTasks)).ToList();
    }
}
