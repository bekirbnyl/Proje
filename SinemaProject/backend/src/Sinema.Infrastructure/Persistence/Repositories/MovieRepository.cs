using Microsoft.EntityFrameworkCore;
using Sinema.Domain.Entities;
using Sinema.Domain.Interfaces.Repositories;

namespace Sinema.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Movie entity
/// </summary>
public class MovieRepository : IMovieRepository
{
    private readonly SinemaDbContext _context;

    public MovieRepository(SinemaDbContext context)
    {
        _context = context;
    }

    public async Task<Movie?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Movies
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Movie>> GetActiveMoviesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Movies
            .Where(m => m.IsActive)
            .OrderBy(m => m.Title)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Movie>> GetAllMoviesAsync(bool? activeOnly = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Movies.AsQueryable();

        if (activeOnly.HasValue)
        {
            query = query.Where(m => m.IsActive == activeOnly.Value);
        }

        return await query
            .OrderBy(m => m.Title)
            .ToListAsync(cancellationToken);
    }

    public async Task<Movie?> GetByTitleAsync(string title, CancellationToken cancellationToken = default)
    {
        return await _context.Movies
            .FirstOrDefaultAsync(m => m.Title == title, cancellationToken);
    }

    public async Task<bool> HasFutureScreeningsAsync(Guid movieId, CancellationToken cancellationToken = default)
    {
        return await _context.Screenings
            .AnyAsync(s => s.MovieId == movieId && s.StartAt > DateTime.UtcNow, cancellationToken);
    }

    public async Task AddAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        _context.Movies.Add(movie);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        _context.Movies.Update(movie);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        _context.Movies.Remove(movie);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
