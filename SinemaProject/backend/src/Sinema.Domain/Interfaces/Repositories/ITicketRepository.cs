using Sinema.Domain.Entities;
using Sinema.Domain.Enums;

namespace Sinema.Domain.Interfaces.Repositories;

public interface ITicketRepository
{
    Task<Ticket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Ticket>> GetByScreeningIdAsync(Guid screeningId, CancellationToken cancellationToken = default);
    Task<Ticket?> GetByScreeningAndSeatAsync(Guid screeningId, Guid seatId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Ticket>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<Ticket>> GetByChannelAsync(TicketChannel channel, CancellationToken cancellationToken = default);
    Task<bool> ExistsByTicketCodeAsync(string ticketCode, CancellationToken cancellationToken = default);
    Task AddAsync(Ticket ticket, CancellationToken cancellationToken = default);
    Task UpdateAsync(Ticket ticket, CancellationToken cancellationToken = default);
    Task DeleteAsync(Ticket ticket, CancellationToken cancellationToken = default);
}
