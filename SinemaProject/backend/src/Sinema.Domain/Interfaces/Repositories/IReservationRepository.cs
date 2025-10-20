using Sinema.Domain.Entities;
using Sinema.Domain.Enums;

namespace Sinema.Domain.Interfaces.Repositories;

public interface IReservationRepository
{
    Task<Reservation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Reservation>> GetByReservationIdAsync(Guid reservationId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Reservation>> GetByScreeningIdAsync(Guid screeningId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Reservation>> GetExpiredReservationsAsync(DateTime currentTime, CancellationToken cancellationToken = default);
    Task<Reservation?> GetByScreeningAndSeatAsync(Guid screeningId, Guid seatId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Reservation>> GetByMemberIdAsync(Guid memberId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Reservation>> GetByStatusAsync(ReservationStatus status, CancellationToken cancellationToken = default);
    Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default);
    Task UpdateAsync(Reservation reservation, CancellationToken cancellationToken = default);
    Task DeleteAsync(Reservation reservation, CancellationToken cancellationToken = default);
}
