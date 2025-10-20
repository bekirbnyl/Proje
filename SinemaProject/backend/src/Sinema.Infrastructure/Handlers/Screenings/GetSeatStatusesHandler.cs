using MediatR;
using Sinema.Application.Abstractions.Seating;
using Sinema.Application.DTOs.Seating;
using Sinema.Application.Features.Screenings.Queries.GetSeatStatuses;

namespace Sinema.Infrastructure.Handlers.Screenings;

/// <summary>
/// Handler for getting seat statuses for a screening
/// </summary>
public class GetSeatStatusesHandler : IRequestHandler<GetSeatStatusesQuery, SeatGridResponse>
{
    private readonly ISeatStatusService _seatStatusService;

    public GetSeatStatusesHandler(ISeatStatusService seatStatusService)
    {
        _seatStatusService = seatStatusService;
    }

    public async Task<SeatGridResponse> Handle(GetSeatStatusesQuery request, CancellationToken cancellationToken)
    {
        return await _seatStatusService.GetSeatGridAsync(request.ScreeningId, cancellationToken);
    }
}
