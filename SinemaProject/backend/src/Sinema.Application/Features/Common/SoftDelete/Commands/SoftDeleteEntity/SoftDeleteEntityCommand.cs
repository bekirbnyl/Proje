using MediatR;

namespace Sinema.Application.Features.Common.SoftDelete.Commands.SoftDeleteEntity;

/// <summary>
/// Command to soft-delete an entity with validation and audit logging
/// </summary>
public record SoftDeleteEntityCommand : IRequest<bool>
{
    /// <summary>
    /// Type name of the entity to delete
    /// </summary>
    public string EntityType { get; init; } = string.Empty;

    /// <summary>
    /// ID of the entity to delete
    /// </summary>
    public Guid EntityId { get; init; }

    /// <summary>
    /// Reason for the deletion (required)
    /// </summary>
    public string Reason { get; init; } = string.Empty;

    /// <summary>
    /// ID of the user performing the deletion
    /// </summary>
    public Guid? UserId { get; init; }

    /// <summary>
    /// ID of the supervisor who approved the deletion (if required)
    /// </summary>
    public Guid? ApprovedBy { get; init; }
}
