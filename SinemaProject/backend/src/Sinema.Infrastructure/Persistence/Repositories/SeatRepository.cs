using Microsoft.EntityFrameworkCore;
using Sinema.Domain.Entities;
using Sinema.Domain.Interfaces.Repositories;

namespace Sinema.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Seat entity
/// </summary>
public class SeatRepository : ISeatRepository
{
    private readonly SinemaDbContext _context;

    public SeatRepository(SinemaDbContext context)
    {
        _context = context;
    }

    public async Task<Seat?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Seats
            .Include(s => s.SeatLayout)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Seat>> GetBySeatLayoutIdAsync(Guid seatLayoutId, CancellationToken cancellationToken = default)
    {
        return await _context.Seats
            .Where(s => s.SeatLayoutId == seatLayoutId)
            .OrderBy(s => s.Row)
            .ThenBy(s => s.Col)
            .ToListAsync(cancellationToken);
    }

    public async Task<Seat?> GetByPositionAsync(Guid seatLayoutId, int row, int col, CancellationToken cancellationToken = default)
    {
        return await _context.Seats
            .FirstOrDefaultAsync(s => s.SeatLayoutId == seatLayoutId && s.Row == row && s.Col == col, cancellationToken);
    }

    public async Task AddAsync(Seat seat, CancellationToken cancellationToken = default)
    {
        _context.Seats.Add(seat);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<Seat> seats, CancellationToken cancellationToken = default)
    {
        _context.Seats.AddRange(seats);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Seat seat, CancellationToken cancellationToken = default)
    {
        _context.Seats.Update(seat);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Seat seat, CancellationToken cancellationToken = default)
    {
        _context.Seats.Remove(seat);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
