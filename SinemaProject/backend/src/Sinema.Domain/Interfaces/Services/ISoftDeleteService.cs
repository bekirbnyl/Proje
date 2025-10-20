namespace Sinema.Domain.Interfaces.Services;

/// <summary>
/// Service for managing soft-delete operations with dependency validation
/// </summary>
public interface ISoftDeleteService
{
    /// <summary>
    /// Performs soft-delete on an entity with dependency validation
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="entityId">Entity ID</param>
    /// <param name="reason">Deletion reason</param>
    /// <param name="userId">ID of the user performing the deletion</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful, false if entity not found</returns>
    /// <exception cref="InvalidOperationException">Thrown when deletion is not allowed due to dependencies</exception>
    Task<bool> SoftDeleteAsync<T>(Guid entityId, string reason, Guid? userId = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Restores a soft-deleted entity with dependency validation
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="entityId">Entity ID</param>
    /// <param name="userId">ID of the user performing the restore</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful, false if entity not found</returns>
    /// <exception cref="InvalidOperationException">Thrown when restore is not allowed due to dependencies</exception>
    Task<bool> RestoreAsync<T>(Guid entityId, Guid? userId = null, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Validates if an entity can be soft-deleted
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="entityId">Entity ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result with error messages if any</returns>
    Task<(bool CanDelete, string[] Errors)> ValidateDeleteAsync<T>(Guid entityId, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Validates if an entity can be restored
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="entityId">Entity ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result with error messages if any</returns>
    Task<(bool CanRestore, string[] Errors)> ValidateRestoreAsync<T>(Guid entityId, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Checks if an entity is soft-deleted
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="entityId">Entity ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if soft-deleted</returns>
    Task<bool> IsDeletedAsync<T>(Guid entityId, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Gets the deletion information for a soft-deleted entity
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <param name="entityId">Entity ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Deletion information or null if not found or not deleted</returns>
    Task<(DateTime? DeletedAt, Guid? DeletedBy)?> GetDeletionInfoAsync<T>(Guid entityId, CancellationToken cancellationToken = default) where T : class;
}
