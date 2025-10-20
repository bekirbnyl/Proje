using Microsoft.EntityFrameworkCore;
using Sinema.Domain.Entities;
using Sinema.Domain.Interfaces.Repositories;

namespace Sinema.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Hall entity
/// </summary>
public class HallRepository : IHallRepository
{
    private readonly SinemaDbContext _context;

    public HallRepository(SinemaDbContext context)
    {
        _context = context;
    }

    public async Task<Hall?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Halls
            .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Hall>> GetByCinemaIdAsync(Guid cinemaId, CancellationToken cancellationToken = default)
    {
        return await _context.Halls
            .Where(h => h.CinemaId == cinemaId)
            .OrderBy(h => h.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Hall?> GetByNameAsync(Guid cinemaId, string name, CancellationToken cancellationToken = default)
    {
        return await _context.Halls
            .FirstOrDefaultAsync(h => h.CinemaId == cinemaId && h.Name == name, cancellationToken);
    }

    public async Task AddAsync(Hall hall, CancellationToken cancellationToken = default)
    {
        _context.Halls.Add(hall);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Hall hall, CancellationToken cancellationToken = default)
    {
        _context.Halls.Update(hall);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Hall hall, CancellationToken cancellationToken = default)
    {
        _context.Halls.Remove(hall);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
