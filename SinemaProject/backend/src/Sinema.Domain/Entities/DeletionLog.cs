namespace Sinema.Domain.Entities;

/// <summary>
/// Represents an audit log entry for entity deletions
/// </summary>
public class DeletionLog
{
    /// <summary>
    /// Unique identifier for the deletion log entry
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Name of the entity type that was deleted
    /// </summary>
    public string EntityName { get; set; } = string.Empty;

    /// <summary>
    /// ID of the entity that was deleted
    /// </summary>
    public Guid EntityId { get; set; }

    /// <summary>
    /// Reason for the deletion
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// ID of the user who performed the deletion
    /// </summary>
    public Guid? DeletedBy { get; set; }

    /// <summary>
    /// ID of the supervisor who approved the deletion (if required)
    /// </summary>
    public Guid? ApprovedBy { get; set; }

    /// <summary>
    /// When the deletion was performed
    /// </summary>
    public DateTime DeletedAt { get; set; }

    /// <summary>
    /// Additional metadata about the deletion (JSON format)
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// Creates a deletion log entry
    /// </summary>
    /// <param name="entityName">Entity type name</param>
    /// <param name="entityId">Entity ID</param>
    /// <param name="reason">Deletion reason</param>
    /// <param name="deletedBy">Who deleted it</param>
    /// <param name="approvedBy">Who approved the deletion</param>
    /// <returns>DeletionLog instance</returns>
    public static DeletionLog Create(string entityName, Guid entityId, string reason, Guid? deletedBy = null, Guid? approvedBy = null)
    {
        return new DeletionLog
        {
            Id = Guid.NewGuid(),
            EntityName = entityName,
            EntityId = entityId,
            Reason = reason,
            DeletedBy = deletedBy,
            ApprovedBy = approvedBy,
            DeletedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Gets the entity identifier as a string for display
    /// </summary>
    public string GetEntityDisplay() => $"{EntityName}:{EntityId}";

    /// <summary>
    /// Checks if the deletion was approved by a supervisor
    /// </summary>
    public bool WasApproved => ApprovedBy.HasValue;
}
