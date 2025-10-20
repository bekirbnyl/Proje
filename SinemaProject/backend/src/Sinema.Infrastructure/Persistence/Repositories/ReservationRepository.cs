using Microsoft.EntityFrameworkCore;
using Sinema.Domain.Entities;
using Sinema.Domain.Enums;
using Sinema.Domain.Interfaces.Repositories;

namespace Sinema.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Reservation entity
/// </summary>
public class ReservationRepository : IReservationRepository
{
    private readonly SinemaDbContext _context;

    public ReservationRepository(SinemaDbContext context)
    {
        _context = context;
    }

    public async Task<Reservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Reservations
            .Include(r => r.Screening)
            .Include(r => r.Seat)
            .Include(r => r.Member)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Reservation>> GetByScreeningIdAsync(Guid screeningId, CancellationToken cancellationToken = default)
    {
        return await _context.Reservations
            .Include(r => r.Seat)
            .Include(r => r.Member)
            .Where(r => r.ScreeningId == screeningId)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Reservation>> GetExpiredReservationsAsync(DateTime currentTime, CancellationToken cancellationToken = default)
    {
        return await _context.Reservations
            .Where(r => r.Status == ReservationStatus.Pending && r.ExpiresAt <= currentTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<Reservation?> GetByScreeningAndSeatAsync(Guid screeningId, Guid seatId, CancellationToken cancellationToken = default)
    {
        return await _context.Reservations
            .Include(r => r.Screening)
            .Include(r => r.Seat)
            .Include(r => r.Member)
            .FirstOrDefaultAsync(r => r.ScreeningId == screeningId && r.SeatId == seatId, cancellationToken);
    }

    public async Task<IEnumerable<Reservation>> GetByMemberIdAsync(Guid memberId, CancellationToken cancellationToken = default)
    {
        return await _context.Reservations
            .Include(r => r.Screening)
            .ThenInclude(s => s.Movie)
            .Include(r => r.Screening)
            .ThenInclude(s => s.Hall)
            .Include(r => r.Seat)
            .Where(r => r.MemberId == memberId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Reservation>> GetByStatusAsync(ReservationStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Reservations
            .Include(r => r.Screening)
            .Include(r => r.Seat)
            .Include(r => r.Member)
            .Where(r => r.Status == status)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default)
    {
        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Reservation reservation, CancellationToken cancellationToken = default)
    {
        _context.Reservations.Update(reservation);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Reservation reservation, CancellationToken cancellationToken = default)
    {
        _context.Reservations.Remove(reservation);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Reservation>> GetByReservationIdAsync(Guid reservationId, CancellationToken cancellationToken = default)
    {
        return await _context.Reservations
            .Include(r => r.Screening)
            .Include(r => r.Seat)
            .Include(r => r.Member)
            .Where(r => r.Id == reservationId)
            .ToListAsync(cancellationToken);
    }
}
