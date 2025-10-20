using Microsoft.EntityFrameworkCore;
using Sinema.Domain.Entities;
using Sinema.Domain.Interfaces.Repositories;

namespace Sinema.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for SettingsChangeLog entity
/// </summary>
public class SettingsChangeLogRepository : ISettingsChangeLogRepository
{
    private readonly SinemaDbContext _context;

    public SettingsChangeLogRepository(SinemaDbContext context)
    {
        _context = context;
    }

    public async Task<SettingsChangeLog> CreateAsync(SettingsChangeLog changeLog, CancellationToken cancellationToken = default)
    {
        _context.SettingsChangeLogs.Add(changeLog);
        await _context.SaveChangesAsync(cancellationToken);
        return changeLog;
    }

    public async Task<IEnumerable<SettingsChangeLog>> CreateManyAsync(IEnumerable<SettingsChangeLog> changeLogs, CancellationToken cancellationToken = default)
    {
        var changeLogsList = changeLogs.ToList();
        
        _context.SettingsChangeLogs.AddRange(changeLogsList);
        await _context.SaveChangesAsync(cancellationToken);
        
        return changeLogsList;
    }

    public async Task<IEnumerable<SettingsChangeLog>> GetByKeyAsync(string key, int limit = 50, CancellationToken cancellationToken = default)
    {
        return await _context.SettingsChangeLogs
            .AsNoTracking()
            .Where(scl => scl.Key == key)
            .OrderByDescending(scl => scl.ChangedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<SettingsChangeLog>> GetByUserAsync(Guid userId, int limit = 100, CancellationToken cancellationToken = default)
    {
        return await _context.SettingsChangeLogs
            .AsNoTracking()
            .Where(scl => scl.ChangedBy == userId)
            .OrderByDescending(scl => scl.ChangedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<SettingsChangeLog>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, int limit = 1000, CancellationToken cancellationToken = default)
    {
        return await _context.SettingsChangeLogs
            .AsNoTracking()
            .Where(scl => scl.ChangedAt >= fromDate && scl.ChangedAt < toDate)
            .OrderByDescending(scl => scl.ChangedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<SettingsChangeLog>> GetRecentAsync(int limit = 100, CancellationToken cancellationToken = default)
    {
        return await _context.SettingsChangeLogs
            .AsNoTracking()
            .OrderByDescending(scl => scl.ChangedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
}
