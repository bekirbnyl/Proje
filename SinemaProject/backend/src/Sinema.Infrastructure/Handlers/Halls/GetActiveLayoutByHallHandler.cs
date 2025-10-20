using MediatR;
using Microsoft.EntityFrameworkCore;
using Sinema.Application.DTOs.Seating;
using Sinema.Application.Features.Halls.Queries.GetActiveLayoutByHall;
using Sinema.Infrastructure.Persistence;

namespace Sinema.Infrastructure.Handlers.Halls;

/// <summary>
/// Handler for getting the active seat layout for a hall
/// </summary>
public class GetActiveLayoutByHallHandler : IRequestHandler<GetActiveLayoutByHallQuery, SeatLayoutDto?>
{
    private readonly SinemaDbContext _context;

    public GetActiveLayoutByHallHandler(SinemaDbContext context)
    {
        _context = context;
    }

    public async Task<SeatLayoutDto?> Handle(GetActiveLayoutByHallQuery request, CancellationToken cancellationToken)
    {
        var activeLayout = await _context.SeatLayouts
            .Include(sl => sl.Seats)
            .FirstOrDefaultAsync(sl => sl.HallId == request.HallId && sl.IsActive, cancellationToken);

        if (activeLayout == null)
        {
            return null;
        }

        return new SeatLayoutDto
        {
            Id = activeLayout.Id,
            HallId = activeLayout.HallId,
            Version = activeLayout.Version,
            IsActive = activeLayout.IsActive,
            CreatedAt = activeLayout.CreatedAt,
            Seats = activeLayout.Seats
                .OrderBy(s => s.Row)
                .ThenBy(s => s.Col)
                .Select(s => new SeatDto
                {
                    Id = s.Id,
                    Row = s.Row,
                    Col = s.Col,
                    Label = s.Label
                })
        };
    }
}
