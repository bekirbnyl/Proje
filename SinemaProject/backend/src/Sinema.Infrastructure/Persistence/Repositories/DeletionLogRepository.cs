using Microsoft.EntityFrameworkCore;
using Sinema.Domain.Entities;
using Sinema.Domain.Interfaces.Repositories;

namespace Sinema.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for DeletionLog entity
/// </summary>
public class DeletionLogRepository : IDeletionLogRepository
{
    private readonly SinemaDbContext _context;

    public DeletionLogRepository(SinemaDbContext context)
    {
        _context = context;
    }

    public async Task<DeletionLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.DeletionLogs
            .FirstOrDefaultAsync(dl => dl.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<DeletionLog>> GetByEntityAsync(string entityName, Guid entityId, int limit = 50, CancellationToken cancellationToken = default)
    {
        return await _context.DeletionLogs
            .AsNoTracking()
            .Where(dl => dl.EntityName == entityName && dl.EntityId == entityId)
            .OrderByDescending(dl => dl.DeletedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<DeletionLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, int limit = 1000, CancellationToken cancellationToken = default)
    {
        return await _context.DeletionLogs
            .AsNoTracking()
            .Where(dl => dl.DeletedAt >= startDate && dl.DeletedAt < endDate)
            .OrderByDescending(dl => dl.DeletedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<DeletionLog>> GetByDeletedByAsync(Guid deletedBy, int limit = 100, CancellationToken cancellationToken = default)
    {
        return await _context.DeletionLogs
            .AsNoTracking()
            .Where(dl => dl.DeletedBy == deletedBy)
            .OrderByDescending(dl => dl.DeletedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<DeletionLog>> GetRecentAsync(int limit = 100, CancellationToken cancellationToken = default)
    {
        return await _context.DeletionLogs
            .AsNoTracking()
            .OrderByDescending(dl => dl.DeletedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<DeletionLog> CreateAsync(DeletionLog deletionLog, CancellationToken cancellationToken = default)
    {
        _context.DeletionLogs.Add(deletionLog);
        await _context.SaveChangesAsync(cancellationToken);
        return deletionLog;
    }

    public async Task AddAsync(DeletionLog deletionLog, CancellationToken cancellationToken = default)
    {
        _context.DeletionLogs.Add(deletionLog);
        await _context.SaveChangesAsync(cancellationToken);
    }

}
