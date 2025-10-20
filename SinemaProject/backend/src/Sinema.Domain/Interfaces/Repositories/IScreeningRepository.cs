using Sinema.Domain.Entities;

namespace Sinema.Domain.Interfaces.Repositories;

public interface IScreeningRepository
{
    Task<Screening?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Screening?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Screening>> GetByHallAndDateRangeAsync(Guid hallId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<Screening>> GetByMovieIdAsync(Guid movieId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Screening>> GetUpcomingScreeningsAsync(DateTime fromDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<Screening>> GetFilteredScreeningsAsync(DateTime? date = null, Guid? hallId = null, Guid? movieId = null, CancellationToken cancellationToken = default);
    Task<bool> HasConflictingScreeningsAsync(Guid hallId, DateTime startTime, DateTime endTime, Guid? excludeScreeningId = null, CancellationToken cancellationToken = default);
    Task AddAsync(Screening screening, CancellationToken cancellationToken = default);
    Task UpdateAsync(Screening screening, CancellationToken cancellationToken = default);
    Task DeleteAsync(Screening screening, CancellationToken cancellationToken = default);
}
