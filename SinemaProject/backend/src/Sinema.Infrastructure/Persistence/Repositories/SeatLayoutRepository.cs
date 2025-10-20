using Microsoft.EntityFrameworkCore;
using Sinema.Domain.Entities;
using Sinema.Domain.Interfaces.Repositories;

namespace Sinema.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for SeatLayout entity
/// </summary>
public class SeatLayoutRepository : ISeatLayoutRepository
{
    private readonly SinemaDbContext _context;

    public SeatLayoutRepository(SinemaDbContext context)
    {
        _context = context;
    }

    public async Task<SeatLayout?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.SeatLayouts
            .FirstOrDefaultAsync(sl => sl.Id == id, cancellationToken);
    }

    public async Task<SeatLayout?> GetActiveByHallIdAsync(Guid hallId, CancellationToken cancellationToken = default)
    {
        return await _context.SeatLayouts
            .Where(sl => sl.HallId == hallId && sl.IsActive)
            .OrderByDescending(sl => sl.Version)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<SeatLayout>> GetByHallIdAsync(Guid hallId, CancellationToken cancellationToken = default)
    {
        return await _context.SeatLayouts
            .Where(sl => sl.HallId == hallId)
            .OrderByDescending(sl => sl.Version)
            .ToListAsync(cancellationToken);
    }

    public async Task<SeatLayout?> GetByHallAndVersionAsync(Guid hallId, int version, CancellationToken cancellationToken = default)
    {
        return await _context.SeatLayouts
            .FirstOrDefaultAsync(sl => sl.HallId == hallId && sl.Version == version, cancellationToken);
    }

    public async Task AddAsync(SeatLayout seatLayout, CancellationToken cancellationToken = default)
    {
        _context.SeatLayouts.Add(seatLayout);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(SeatLayout seatLayout, CancellationToken cancellationToken = default)
    {
        _context.SeatLayouts.Update(seatLayout);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(SeatLayout seatLayout, CancellationToken cancellationToken = default)
    {
        _context.SeatLayouts.Remove(seatLayout);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
