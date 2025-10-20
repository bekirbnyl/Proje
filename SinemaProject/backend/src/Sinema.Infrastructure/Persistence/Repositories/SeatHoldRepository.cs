using Microsoft.EntityFrameworkCore;
using Sinema.Domain.Entities;
using Sinema.Domain.Interfaces.Repositories;
using Sinema.Infrastructure.Persistence;

namespace Sinema.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for seat hold operations
/// </summary>
public class SeatHoldRepository : ISeatHoldRepository
{
    private readonly SinemaDbContext _context;

    public SeatHoldRepository(SinemaDbContext context)
    {
        _context = context;
    }

    public async Task<SeatHold?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.SeatHolds
            .Include(h => h.Screening)
            .Include(h => h.Seat)
            .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<SeatHold>> GetActiveHoldsForSeatsAsync(Guid screeningId, IEnumerable<Guid> seatIds, CancellationToken cancellationToken = default)
    {
        var currentTime = DateTime.UtcNow;
        var seatIdList = seatIds.ToList();

        return await _context.SeatHolds
            .Where(h => h.ScreeningId == screeningId &&
                       seatIdList.Contains(h.SeatId) &&
                       h.ExpiresAt > currentTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<SeatHold>> GetActiveHoldsBySeatAsync(Guid screeningId, Guid seatId, CancellationToken cancellationToken = default)
    {
        var currentTime = DateTime.UtcNow;

        return await _context.SeatHolds
            .Where(h => h.ScreeningId == screeningId &&
                       h.SeatId == seatId &&
                       h.ExpiresAt > currentTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<SeatHold>> GetActiveHoldsByScreeningAsync(Guid screeningId, CancellationToken cancellationToken = default)
    {
        var currentTime = DateTime.UtcNow;

        return await _context.SeatHolds
            .Where(h => h.ScreeningId == screeningId &&
                       h.ExpiresAt > currentTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<SeatHold>> GetByClientTokenAsync(string clientToken, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.SeatHolds
            .Where(h => h.ClientToken == clientToken);

        if (userId.HasValue)
        {
            query = query.Where(h => h.UserId == userId.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<SeatHold>> GetExpiredHoldsAsync(DateTime currentTime, int batchSize = 100, CancellationToken cancellationToken = default)
    {
        return await _context.SeatHolds
            .Where(h => h.ExpiresAt <= currentTime)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<SeatHold> holds, CancellationToken cancellationToken = default)
    {
        await _context.SeatHolds.AddRangeAsync(holds, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(SeatHold hold, CancellationToken cancellationToken = default)
    {
        _context.SeatHolds.Update(hold);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(SeatHold hold, CancellationToken cancellationToken = default)
    {
        _context.SeatHolds.Remove(hold);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveRangeAsync(IEnumerable<SeatHold> holds, CancellationToken cancellationToken = default)
    {
        _context.SeatHolds.RemoveRange(holds);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> RemoveHoldsBySeatsAsync(Guid screeningId, IEnumerable<Guid> seatIds, string clientToken, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var seatIdList = seatIds.ToList();
        
        var holdsToRemove = await _context.SeatHolds
            .Where(h => h.ScreeningId == screeningId &&
                       seatIdList.Contains(h.SeatId) &&
                       h.ClientToken == clientToken)
            .ToListAsync(cancellationToken);

        // Additional filtering for user ownership if specified
        if (userId.HasValue)
        {
            holdsToRemove = holdsToRemove
                .Where(h => h.UserId == userId.Value)
                .ToList();
        }

        if (holdsToRemove.Any())
        {
            _context.SeatHolds.RemoveRange(holdsToRemove);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return holdsToRemove.Count;
    }
}
