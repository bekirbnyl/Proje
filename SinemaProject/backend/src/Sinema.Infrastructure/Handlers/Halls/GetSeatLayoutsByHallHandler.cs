using MediatR;
using Microsoft.EntityFrameworkCore;
using Sinema.Application.DTOs.Seating;
using Sinema.Application.Features.Halls.Queries.GetSeatLayoutsByHall;
using Sinema.Infrastructure.Persistence;

namespace Sinema.Infrastructure.Handlers.Halls;

/// <summary>
/// Handler for getting all seat layout versions for a hall
/// </summary>
public class GetSeatLayoutsByHallHandler : IRequestHandler<GetSeatLayoutsByHallQuery, IEnumerable<SeatLayoutDto>>
{
    private readonly SinemaDbContext _context;

    public GetSeatLayoutsByHallHandler(SinemaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SeatLayoutDto>> Handle(GetSeatLayoutsByHallQuery request, CancellationToken cancellationToken)
    {
        var layouts = await _context.SeatLayouts
            .Include(sl => sl.Seats)
            .Where(sl => sl.HallId == request.HallId)
            .OrderByDescending(sl => sl.Version) // Latest version first
            .ToListAsync(cancellationToken);

        return layouts.Select(layout => new SeatLayoutDto
        {
            Id = layout.Id,
            HallId = layout.HallId,
            Version = layout.Version,
            IsActive = layout.IsActive,
            CreatedAt = layout.CreatedAt,
            Seats = layout.Seats
                .OrderBy(s => s.Row)
                .ThenBy(s => s.Col)
                .Select(s => new SeatDto
                {
                    Id = s.Id,
                    Row = s.Row,
                    Col = s.Col,
                    Label = s.Label
                })
        });
    }
}
