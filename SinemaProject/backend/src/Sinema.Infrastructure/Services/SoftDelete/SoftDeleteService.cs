using Microsoft.EntityFrameworkCore;
using Sinema.Domain.Entities;
using Sinema.Domain.Interfaces.Services;
using Sinema.Infrastructure.Persistence;
using Sinema.Infrastructure.Persistence.QueryFilters;
using System.Reflection;

namespace Sinema.Infrastructure.Services;

/// <summary>
/// Service implementation for managing soft-delete operations with dependency validation
/// </summary>
public class SoftDeleteService : ISoftDeleteService
{
    private readonly SinemaDbContext _context;
    private readonly IAuditLogger _auditLogger;

    public SoftDeleteService(SinemaDbContext context, IAuditLogger auditLogger)
    {
        _context = context;
        _auditLogger = auditLogger;
    }

    public async Task<bool> SoftDeleteAsync<T>(Guid entityId, string reason, Guid? userId = null, CancellationToken cancellationToken = default) where T : class
    {
        // Validate deletion first
        var validation = await ValidateDeleteAsync<T>(entityId, cancellationToken);
        if (!validation.CanDelete)
        {
            throw new InvalidOperationException($"Cannot delete {typeof(T).Name}: {string.Join(", ", validation.Errors)}");
        }

        var entity = await _context.Set<T>()
            .IncludeDeleted()
            .FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == entityId, cancellationToken);

        if (entity == null)
        {
            return false;
        }

        // Check if already deleted
        var isDeleted = (bool)typeof(T).GetProperty("IsDeleted")!.GetValue(entity)!;
        if (isDeleted)
        {
            throw new InvalidOperationException($"{typeof(T).Name} is already deleted");
        }

        // Set soft delete properties
        typeof(T).GetProperty("IsDeleted")!.SetValue(entity, true);
        typeof(T).GetProperty("DeletedAt")!.SetValue(entity, DateTime.UtcNow);
        typeof(T).GetProperty("DeletedBy")!.SetValue(entity, userId);

        await _context.SaveChangesAsync(cancellationToken);

        // Log the deletion
        await _auditLogger.LogDeletionAsync(typeof(T).Name, entityId, reason, userId, cancellationToken: cancellationToken);

        return true;
    }

    public async Task<bool> RestoreAsync<T>(Guid entityId, Guid? userId = null, CancellationToken cancellationToken = default) where T : class
    {
        // Validate restore first
        var validation = await ValidateRestoreAsync<T>(entityId, cancellationToken);
        if (!validation.CanRestore)
        {
            throw new InvalidOperationException($"Cannot restore {typeof(T).Name}: {string.Join(", ", validation.Errors)}");
        }

        var entity = await _context.Set<T>()
            .IncludeDeleted()
            .FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == entityId, cancellationToken);

        if (entity == null)
        {
            return false;
        }

        // Check if actually deleted
        var isDeleted = (bool)typeof(T).GetProperty("IsDeleted")!.GetValue(entity)!;
        if (!isDeleted)
        {
            throw new InvalidOperationException($"{typeof(T).Name} is not deleted");
        }

        // Restore entity
        typeof(T).GetProperty("IsDeleted")!.SetValue(entity, false);
        typeof(T).GetProperty("DeletedAt")!.SetValue(entity, null);
        typeof(T).GetProperty("DeletedBy")!.SetValue(entity, null);

        await _context.SaveChangesAsync(cancellationToken);

        // Log the restoration
        await _auditLogger.LogRestoreAsync(typeof(T).Name, entityId, userId, cancellationToken: cancellationToken);

        return true;
    }

    public async Task<(bool CanDelete, string[] Errors)> ValidateDeleteAsync<T>(Guid entityId, CancellationToken cancellationToken = default) where T : class
    {
        var errors = new List<string>();

        if (typeof(T) == typeof(Movie))
        {
            // Check for future screenings
            var futureScreenings = await _context.Screenings
                .Where(s => s.MovieId == entityId && s.StartAt > DateTime.UtcNow && !s.IsDeleted)
                .CountAsync(cancellationToken);

            if (futureScreenings > 0)
            {
                errors.Add($"Cannot delete movie: {futureScreenings} future screening(s) exist");
            }
        }
        else if (typeof(T) == typeof(Hall))
        {
            // Check for future screenings
            var futureScreenings = await _context.Screenings
                .Where(s => s.HallId == entityId && s.StartAt > DateTime.UtcNow && !s.IsDeleted)
                .CountAsync(cancellationToken);

            if (futureScreenings > 0)
            {
                errors.Add($"Cannot delete hall: {futureScreenings} future screening(s) exist");
            }
        }
        else if (typeof(T) == typeof(SeatLayout))
        {
            // Check for future screenings using this layout
            var futureScreenings = await _context.Screenings
                .Where(s => s.SeatLayoutId == entityId && s.StartAt > DateTime.UtcNow && !s.IsDeleted)
                .CountAsync(cancellationToken);

            if (futureScreenings > 0)
            {
                errors.Add($"Cannot delete seat layout: {futureScreenings} future screening(s) use this layout");
            }
        }
        else if (typeof(T) == typeof(Screening))
        {
            // Check for existing reservations or tickets
            var reservations = await _context.Reservations
                .Where(r => r.ScreeningId == entityId)
                .CountAsync(cancellationToken);

            var tickets = await _context.Tickets
                .Where(t => t.ScreeningId == entityId)
                .CountAsync(cancellationToken);

            if (reservations > 0)
            {
                errors.Add($"Cannot delete screening: {reservations} reservation(s) exist");
            }

            if (tickets > 0)
            {
                errors.Add($"Cannot delete screening: {tickets} ticket(s) sold");
            }
        }

        return (errors.Count == 0, errors.ToArray());
    }

    public async Task<(bool CanRestore, string[] Errors)> ValidateRestoreAsync<T>(Guid entityId, CancellationToken cancellationToken = default) where T : class
    {
        var errors = new List<string>();

        if (typeof(T) == typeof(SeatLayout))
        {
            // Check if parent hall is deleted
            var seatLayout = await _context.SeatLayouts
                .IncludeDeleted()
                .Include(sl => sl.Hall)
                .FirstOrDefaultAsync(sl => sl.Id == entityId, cancellationToken);

            if (seatLayout?.Hall.IsDeleted == true)
            {
                errors.Add("Cannot restore seat layout: parent hall is deleted");
            }
        }
        else if (typeof(T) == typeof(Screening))
        {
            // Check if movie, hall, or seat layout are deleted
            var screening = await _context.Screenings
                .IncludeDeleted()
                .Include(s => s.Movie)
                .Include(s => s.Hall)
                .Include(s => s.SeatLayout)
                .FirstOrDefaultAsync(s => s.Id == entityId, cancellationToken);

            if (screening != null)
            {
                if (screening.Movie.IsDeleted)
                {
                    errors.Add("Cannot restore screening: movie is deleted");
                }

                if (screening.Hall.IsDeleted)
                {
                    errors.Add("Cannot restore screening: hall is deleted");
                }

                if (screening.SeatLayout.IsDeleted)
                {
                    errors.Add("Cannot restore screening: seat layout is deleted");
                }
            }
        }

        return (errors.Count == 0, errors.ToArray());
    }

    public async Task<bool> IsDeletedAsync<T>(Guid entityId, CancellationToken cancellationToken = default) where T : class
    {
        var entity = await _context.Set<T>()
            .IncludeDeleted()
            .FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == entityId, cancellationToken);

        if (entity == null)
        {
            return false;
        }

        return (bool)typeof(T).GetProperty("IsDeleted")!.GetValue(entity)!;
    }

    public async Task<(DateTime? DeletedAt, Guid? DeletedBy)?> GetDeletionInfoAsync<T>(Guid entityId, CancellationToken cancellationToken = default) where T : class
    {
        var entity = await _context.Set<T>()
            .IncludeDeleted()
            .FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == entityId, cancellationToken);

        if (entity == null)
        {
            return null;
        }

        var isDeleted = (bool)typeof(T).GetProperty("IsDeleted")!.GetValue(entity)!;
        if (!isDeleted)
        {
            return null;
        }

        var deletedAt = (DateTime?)typeof(T).GetProperty("DeletedAt")!.GetValue(entity);
        var deletedBy = (Guid?)typeof(T).GetProperty("DeletedBy")!.GetValue(entity);

        return (deletedAt, deletedBy);
    }
}
