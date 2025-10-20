using Microsoft.EntityFrameworkCore;
using Sinema.Domain.Entities;
using Sinema.Domain.Interfaces.Repositories;

namespace Sinema.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Screening entity
/// </summary>
public class ScreeningRepository : IScreeningRepository
{
    private readonly SinemaDbContext _context;

    public ScreeningRepository(SinemaDbContext context)
    {
        _context = context;
    }

    public async Task<Screening?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Screenings
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Screening?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Screenings
            .Include(s => s.Movie)
            .Include(s => s.Hall)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Screening>> GetByHallAndDateRangeAsync(Guid hallId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.Screenings
            .Where(s => s.HallId == hallId && s.StartAt >= startDate && s.StartAt < endDate)
            .OrderBy(s => s.StartAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Screening>> GetByMovieIdAsync(Guid movieId, CancellationToken cancellationToken = default)
    {
        return await _context.Screenings
            .Include(s => s.Hall)
            .Where(s => s.MovieId == movieId)
            .OrderBy(s => s.StartAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Screening>> GetUpcomingScreeningsAsync(DateTime fromDate, CancellationToken cancellationToken = default)
    {
        return await _context.Screenings
            .Include(s => s.Movie)
            .Include(s => s.Hall)
            .Where(s => s.StartAt >= fromDate)
            .OrderBy(s => s.StartAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Screening>> GetFilteredScreeningsAsync(DateTime? date = null, Guid? hallId = null, Guid? movieId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Screenings
            .Include(s => s.Movie)
            .Include(s => s.Hall)
                .ThenInclude(h => h.Cinema)
            .AsQueryable();

        // Filter by date if specified
        if (date.HasValue)
        {
            var startDate = date.Value.Date;
            var endDate = startDate.AddDays(1);
            query = query.Where(s => s.StartAt >= startDate && s.StartAt < endDate);
        }

        // Filter by hall if specified
        if (hallId.HasValue)
        {
            query = query.Where(s => s.HallId == hallId.Value);
        }

        // Filter by movie if specified
        if (movieId.HasValue)
        {
            query = query.Where(s => s.MovieId == movieId.Value);
        }

        return await query
            .OrderBy(s => s.StartAt)
            .ThenBy(s => s.Hall.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasConflictingScreeningsAsync(Guid hallId, DateTime startTime, DateTime endTime, Guid? excludeScreeningId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Screenings
            .Where(s => s.HallId == hallId);

        // Exclude the screening being updated if specified
        if (excludeScreeningId.HasValue)
        {
            query = query.Where(s => s.Id != excludeScreeningId.Value);
        }

        // Check for overlap: startTime < other.EndAt && endTime > other.StartAt
        return await query
            .AnyAsync(s => startTime < s.StartAt.AddMinutes(s.DurationMinutes) && endTime > s.StartAt, cancellationToken);
    }

    public async Task AddAsync(Screening screening, CancellationToken cancellationToken = default)
    {
        _context.Screenings.Add(screening);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Screening screening, CancellationToken cancellationToken = default)
    {
        _context.Screenings.Update(screening);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Screening screening, CancellationToken cancellationToken = default)
    {
        _context.Screenings.Remove(screening);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
