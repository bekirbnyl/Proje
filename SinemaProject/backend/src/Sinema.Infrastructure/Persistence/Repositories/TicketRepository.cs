using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Sinema.Domain.Entities;
using Sinema.Domain.Enums;
using Sinema.Domain.Interfaces.Repositories;

namespace Sinema.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Ticket entity
/// </summary>
public class TicketRepository : ITicketRepository
{
    private readonly SinemaDbContext _context;

    public TicketRepository(SinemaDbContext context)
    {
        _context = context;
    }

    public async Task<Ticket?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Tickets
            .Include(t => t.Screening)
            .ThenInclude(s => s.Movie)
            .Include(t => t.Screening)
            .ThenInclude(s => s.Hall)
            .Include(t => t.Seat)
            .Include(t => t.Payment)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Ticket>> GetByScreeningIdAsync(Guid screeningId, CancellationToken cancellationToken = default)
    {
        return await _context.Tickets
            .Include(t => t.Seat)
            .Include(t => t.Payment)
            .Where(t => t.ScreeningId == screeningId)
            .OrderBy(t => t.SoldAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Ticket?> GetByScreeningAndSeatAsync(Guid screeningId, Guid seatId, CancellationToken cancellationToken = default)
    {
        return await _context.Tickets
            .Include(t => t.Screening)
            .Include(t => t.Seat)
            .Include(t => t.Payment)
            .FirstOrDefaultAsync(t => t.ScreeningId == screeningId && t.SeatId == seatId, cancellationToken);
    }

    public async Task<IEnumerable<Ticket>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.Tickets
            .Include(t => t.Screening)
            .ThenInclude(s => s.Movie)
            .Include(t => t.Screening)
            .ThenInclude(s => s.Hall)
            .Include(t => t.Seat)
            .Include(t => t.Payment)
            .Where(t => t.SoldAt >= startDate && t.SoldAt < endDate)
            .OrderBy(t => t.SoldAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Ticket>> GetByChannelAsync(TicketChannel channel, CancellationToken cancellationToken = default)
    {
        return await _context.Tickets
            .Include(t => t.Screening)
            .ThenInclude(s => s.Movie)
            .Include(t => t.Screening)
            .ThenInclude(s => s.Hall)
            .Include(t => t.Seat)
            .Include(t => t.Payment)
            .Where(t => t.Channel == channel)
            .OrderBy(t => t.SoldAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Ticket ticket, CancellationToken cancellationToken = default)
    {
        _context.Tickets.Add(ticket);
        // Note: SaveChanges is handled by UnitOfWork in transaction scenarios
        await Task.CompletedTask;
    }

    public async Task UpdateAsync(Ticket ticket, CancellationToken cancellationToken = default)
    {
        _context.Tickets.Update(ticket);
        // Note: SaveChanges is handled by UnitOfWork in transaction scenarios
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Ticket ticket, CancellationToken cancellationToken = default)
    {
        _context.Tickets.Remove(ticket);
        // Note: SaveChanges is handled by UnitOfWork in transaction scenarios
        await Task.CompletedTask;
    }

    public async Task<bool> ExistsByTicketCodeAsync(string ticketCode, CancellationToken cancellationToken = default)
    {
        return await _context.Tickets
            .AnyAsync(t => t.TicketCode == ticketCode, cancellationToken);
    }
}
