using Microsoft.EntityFrameworkCore;
using Sinema.Application.Abstractions.Seating;
using Sinema.Application.DTOs.Seating;
using Sinema.Domain.Enums;
using Sinema.Infrastructure.Persistence;

namespace Sinema.Infrastructure.Services.Seating;

/// <summary>
/// Service for calculating and retrieving seat status for screenings
/// </summary>
public class SeatStatusService : ISeatStatusService
{
    private readonly SinemaDbContext _context;

    public SeatStatusService(SinemaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SeatStatusDto>> GetSeatStatusesAsync(Guid screeningId, CancellationToken cancellationToken = default)
    {
        // Get the screening with its seat layout
        var screening = await _context.Screenings
            .Include(s => s.SeatLayout)
            .ThenInclude(sl => sl.Seats)
            .FirstOrDefaultAsync(s => s.Id == screeningId, cancellationToken);

        if (screening == null)
        {
            throw new ArgumentException($"Screening with ID {screeningId} not found.", nameof(screeningId));
        }

        // Get all seats for this screening's layout
        var seats = screening.SeatLayout.Seats.ToList();

        // Get sold seats (tickets)
        var soldSeatIds = await _context.Tickets
            .Where(t => t.ScreeningId == screeningId)
            .Select(t => t.SeatId)
            .ToHashSetAsync(cancellationToken);

        // Get reserved seats (active reservations)
        var reservedSeatIds = await _context.Reservations
            .Where(r => r.ScreeningId == screeningId && 
                       (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Confirmed))
            .Select(r => r.SeatId)
            .ToHashSetAsync(cancellationToken);

        // Get held seats (active seat holds)
        var currentTime = DateTime.UtcNow;
        var heldSeatIds = await _context.SeatHolds
            .Where(h => h.ScreeningId == screeningId && h.ExpiresAt > currentTime)
            .Select(h => h.SeatId)
            .ToHashSetAsync(cancellationToken);

        // Build seat status DTOs
        var seatStatuses = seats.Select(seat =>
        {
            string state;
            // Priority: sold > reserved > held > available
            if (soldSeatIds.Contains(seat.Id))
            {
                state = "sold";
            }
            else if (reservedSeatIds.Contains(seat.Id))
            {
                state = "reserved";
            }
            else if (heldSeatIds.Contains(seat.Id))
            {
                state = "held";
            }
            else
            {
                state = "available";
            }

            return new SeatStatusDto
            {
                SeatId = seat.Id,
                Row = seat.Row,
                Col = seat.Col,
                Label = seat.Label,
                State = state
            };
        })
        .OrderBy(s => s.Row)
        .ThenBy(s => s.Col)
        .ToList();

        return seatStatuses;
    }

    public async Task<SeatGridResponse> GetSeatGridAsync(Guid screeningId, CancellationToken cancellationToken = default)
    {
        // Get the screening to get the seat layout ID
        var screening = await _context.Screenings
            .FirstOrDefaultAsync(s => s.Id == screeningId, cancellationToken);

        if (screening == null)
        {
            throw new ArgumentException($"Screening with ID {screeningId} not found.", nameof(screeningId));
        }

        var seatStatuses = await GetSeatStatusesAsync(screeningId, cancellationToken);

        return new SeatGridResponse
        {
            SeatLayoutId = screening.SeatLayoutId,
            Seats = seatStatuses
        };
    }
}
