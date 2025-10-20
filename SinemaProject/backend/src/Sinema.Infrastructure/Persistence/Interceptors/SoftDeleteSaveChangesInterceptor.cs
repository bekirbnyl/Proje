using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Sinema.Application.Abstractions.Authentication;
using Sinema.Domain.Entities;
using System.Reflection;

namespace Sinema.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Interceptor to handle soft delete operations and audit logging
/// </summary>
public class SoftDeleteSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUser _currentUser;
    private readonly IHttpContextAccessor _httpContextAccessor;

    // Entities that support soft delete
    private static readonly HashSet<Type> SoftDeleteSupportedTypes = new()
    {
        typeof(Movie),
        typeof(Hall),
        typeof(SeatLayout),
        typeof(Screening)
    };

    // Entities that should never be hard deleted
    private static readonly HashSet<Type> HardDeleteForbiddenTypes = new()
    {
        typeof(Ticket),
        typeof(Payment),
        typeof(MemberCredit),
        typeof(Reservation)
    };

    public SoftDeleteSaveChangesInterceptor(ICurrentUser currentUser, IHttpContextAccessor httpContextAccessor)
    {
        _currentUser = currentUser;
        _httpContextAccessor = httpContextAccessor;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            ProcessSoftDelete(eventData.Context);
        }
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            ProcessSoftDelete(eventData.Context);
        }
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ProcessSoftDelete(DbContext context)
    {
        var deletedEntries = context.ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Deleted)
            .ToList();

        foreach (var entry in deletedEntries)
        {
            var entityType = entry.Entity.GetType();

            // Check if this entity type is forbidden from hard delete
            if (HardDeleteForbiddenTypes.Contains(entityType))
            {
                throw new InvalidOperationException(
                    $"Hard delete is not allowed for {entityType.Name}. " +
                    "This entity type contains financial or audit-critical data that must be preserved.");
            }

            // Check if this entity supports soft delete
            if (SoftDeleteSupportedTypes.Contains(entityType))
            {
                ProcessSoftDeleteForEntity(context, entry);
            }
        }
    }

    private void ProcessSoftDeleteForEntity(DbContext context, Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
    {
        // Get soft delete properties via reflection
        var isDeletedProperty = entry.Property("IsDeleted");
        var deletedAtProperty = entry.Property("DeletedAt");
        var deletedByProperty = entry.Property("DeletedBy");

        if (isDeletedProperty?.Metadata == null || 
            deletedAtProperty?.Metadata == null || 
            deletedByProperty?.Metadata == null)
        {
            // Entity doesn't have soft delete properties, allow hard delete
            return;
        }

        // Get deletion reason from HTTP context
        var reason = GetDeletionReason();
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new InvalidOperationException(
                $"Deletion reason is required for soft delete operation on {entry.Entity.GetType().Name}");
        }

        // Get current user ID
        var currentUserId = GetCurrentUserId();

        // Convert hard delete to soft delete
        entry.State = EntityState.Modified;
        isDeletedProperty.CurrentValue = true;
        deletedAtProperty.CurrentValue = DateTime.UtcNow;
        deletedByProperty.CurrentValue = currentUserId;

        // Create deletion log entry
        var entityIdProperty = entry.Property("Id");
        if (entityIdProperty?.CurrentValue is Guid entityId)
        {
            var deletionLog = DeletionLog.Create(
                entry.Entity.GetType().Name,
                entityId,
                reason,
                currentUserId);

            context.Set<DeletionLog>().Add(deletionLog);
        }
    }

    private string? GetDeletionReason()
    {
        // Try to get reason from HTTP context items (set by RequireDeleteReasonAttribute)
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.Items.TryGetValue("DeleteReason", out var reasonObj) == true && 
            reasonObj is string reason)
        {
            return reason;
        }

        // Try to get from query string as fallback
        if (httpContext?.Request.Query.TryGetValue("reason", out var reasonQuery) == true)
        {
            return reasonQuery.FirstOrDefault();
        }

        return null;
    }

    private Guid? GetCurrentUserId()
    {
        var userIdString = _currentUser.UserId;
        if (Guid.TryParse(userIdString, out var userId))
        {
            return userId;
        }
        return null;
    }
}
