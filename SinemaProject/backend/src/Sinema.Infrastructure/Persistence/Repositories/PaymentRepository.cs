using Microsoft.EntityFrameworkCore;
using Sinema.Domain.Entities;
using Sinema.Domain.Enums;
using Sinema.Domain.Interfaces.Repositories;

namespace Sinema.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Payment entity
/// </summary>
public class PaymentRepository : IPaymentRepository
{
    private readonly SinemaDbContext _context;

    public PaymentRepository(SinemaDbContext context)
    {
        _context = context;
    }

    public async Task<Payment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Include(p => p.Tickets)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Include(p => p.Tickets)
            .Where(p => p.CreatedAt >= startDate && p.CreatedAt < endDate)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetByMethodAsync(PaymentMethod method, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Include(p => p.Tickets)
            .Where(p => p.Method == method)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Include(p => p.Tickets)
            .Where(p => p.Status == status)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Payment?> GetByExternalReferenceAsync(string externalReference, CancellationToken cancellationToken = default)
    {
        return await _context.Payments
            .Include(p => p.Tickets)
            .FirstOrDefaultAsync(p => p.ExternalReference == externalReference, cancellationToken);
    }

    public async Task AddAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        _context.Payments.Add(payment);
        // Note: SaveChanges is handled by UnitOfWork in transaction scenarios
        await Task.CompletedTask;
    }

    public async Task UpdateAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        _context.Payments.Update(payment);
        // Note: SaveChanges is handled by UnitOfWork in transaction scenarios
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        _context.Payments.Remove(payment);
        // Note: SaveChanges is handled by UnitOfWork in transaction scenarios
        await Task.CompletedTask;
    }
}
