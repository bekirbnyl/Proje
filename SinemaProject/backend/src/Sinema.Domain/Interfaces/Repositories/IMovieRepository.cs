using Sinema.Domain.Entities;

namespace Sinema.Domain.Interfaces.Repositories;

public interface IMovieRepository
{
    Task<Movie?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Movie>> GetActiveMoviesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Movie>> GetAllMoviesAsync(bool? activeOnly = null, CancellationToken cancellationToken = default);
    Task<Movie?> GetByTitleAsync(string title, CancellationToken cancellationToken = default);
    Task<bool> HasFutureScreeningsAsync(Guid movieId, CancellationToken cancellationToken = default);
    Task AddAsync(Movie movie, CancellationToken cancellationToken = default);
    Task UpdateAsync(Movie movie, CancellationToken cancellationToken = default);
    Task DeleteAsync(Movie movie, CancellationToken cancellationToken = default);
}
