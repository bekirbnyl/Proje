using Sinema.Domain.Entities;

namespace Sinema.Domain.Interfaces.Repositories;

public interface ISeatRepository
{
    Task<Seat?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Seat>> GetBySeatLayoutIdAsync(Guid seatLayoutId, CancellationToken cancellationToken = default);
    Task<Seat?> GetByPositionAsync(Guid seatLayoutId, int row, int col, CancellationToken cancellationToken = default);
    Task AddAsync(Seat seat, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<Seat> seats, CancellationToken cancellationToken = default);
    Task UpdateAsync(Seat seat, CancellationToken cancellationToken = default);
    Task DeleteAsync(Seat seat, CancellationToken cancellationToken = default);
}
