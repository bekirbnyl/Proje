using MediatR;

namespace Sinema.Application.Features.Common.SoftDelete.Commands.RestoreEntity;

/// <summary>
/// Command to restore a soft-deleted entity
/// </summary>
public record RestoreEntityCommand : IRequest<bool>
{
    /// <summary>
    /// Type name of the entity to restore
    /// </summary>
    public string EntityType { get; init; } = string.Empty;

    /// <summary>
    /// ID of the entity to restore
    /// </summary>
    public Guid EntityId { get; init; }

    /// <summary>
    /// ID of the user performing the restoration
    /// </summary>
    public Guid? UserId { get; init; }
}
