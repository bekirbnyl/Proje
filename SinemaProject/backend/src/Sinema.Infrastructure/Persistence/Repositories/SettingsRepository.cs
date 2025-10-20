using Microsoft.EntityFrameworkCore;
using Sinema.Domain.Entities;
using Sinema.Domain.Interfaces.Repositories;

namespace Sinema.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Settings entity
/// </summary>
public class SettingsRepository : ISettingsRepository
{
    private readonly SinemaDbContext _context;

    public SettingsRepository(SinemaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Setting>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Settings
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Setting>> GetByKeysAsync(string[] keys, CancellationToken cancellationToken = default)
    {
        return await _context.Settings
            .AsNoTracking()
            .Where(s => keys.Contains(s.Key))
            .ToListAsync(cancellationToken);
    }

    public async Task<Setting?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _context.Settings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Key == key, cancellationToken);
    }

    public async Task<Setting?> GetByKeyWithRowVersionAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _context.Settings
            .FirstOrDefaultAsync(s => s.Key == key, cancellationToken);
    }

    public async Task<Setting> CreateAsync(Setting setting, CancellationToken cancellationToken = default)
    {
        _context.Settings.Add(setting);
        await _context.SaveChangesAsync(cancellationToken);
        return setting;
    }

    public async Task<Setting> UpdateAsync(Setting setting, CancellationToken cancellationToken = default)
    {
        _context.Settings.Update(setting);
        await _context.SaveChangesAsync(cancellationToken);
        return setting;
    }

    public async Task<IEnumerable<Setting>> UpdateManyAsync(IEnumerable<Setting> settings, CancellationToken cancellationToken = default)
    {
        var settingsList = settings.ToList();
        
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            _context.Settings.UpdateRange(settingsList);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            
            return settingsList;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        var setting = await _context.Settings
            .FirstOrDefaultAsync(s => s.Key == key, cancellationToken);
            
        if (setting == null)
            return false;

        _context.Settings.Remove(setting);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _context.Settings
            .AnyAsync(s => s.Key == key, cancellationToken);
    }
}