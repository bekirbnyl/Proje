using MediatR;
using Microsoft.EntityFrameworkCore;
using Sinema.Application.DTOs.Seating;
using Sinema.Application.Features.Halls.Commands.AddSeatLayoutVersion;
using Sinema.Domain.Entities;
using Sinema.Infrastructure.Persistence;

namespace Sinema.Infrastructure.Handlers.Halls;

/// <summary>
/// Handler for adding a new seat layout version to a hall
/// </summary>
public class AddSeatLayoutVersionHandler : IRequestHandler<AddSeatLayoutVersionCommand, SeatLayoutDto>
{
    private readonly SinemaDbContext _context;

    public AddSeatLayoutVersionHandler(SinemaDbContext context)
    {
        _context = context;
    }

    public async Task<SeatLayoutDto> Handle(AddSeatLayoutVersionCommand request, CancellationToken cancellationToken)
    {
        // Verify hall exists
        var hall = await _context.Halls
            .FirstOrDefaultAsync(h => h.Id == request.HallId, cancellationToken);

        if (hall == null)
        {
            throw new ArgumentException($"Hall with ID {request.HallId} not found.", nameof(request.HallId));
        }

        // Get the next version number
        var lastVersion = await _context.SeatLayouts
            .Where(sl => sl.HallId == request.HallId)
            .MaxAsync(sl => (int?)sl.Version, cancellationToken) ?? 0;

        var nextVersion = lastVersion + 1;

        // Create new seat layout (not active by default)
        var seatLayout = new SeatLayout
        {
            Id = Guid.NewGuid(),
            HallId = request.HallId,
            Version = nextVersion,
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.SeatLayouts.Add(seatLayout);

        // Create seats for the layout
        var seats = request.Seats.Select(seatDto => new Seat
        {
            Id = Guid.NewGuid(),
            SeatLayoutId = seatLayout.Id,
            Row = seatDto.Row,
            Col = seatDto.Col,
            Label = seatDto.Label
        }).ToList();

        _context.Seats.AddRange(seats);

        await _context.SaveChangesAsync(cancellationToken);

        // Return the created layout
        return new SeatLayoutDto
        {
            Id = seatLayout.Id,
            HallId = seatLayout.HallId,
            Version = seatLayout.Version,
            IsActive = seatLayout.IsActive,
            CreatedAt = seatLayout.CreatedAt,
            Seats = seats.Select(s => new SeatDto
            {
                Id = s.Id,
                Row = s.Row,
                Col = s.Col,
                Label = s.Label
            })
        };
    }
}
