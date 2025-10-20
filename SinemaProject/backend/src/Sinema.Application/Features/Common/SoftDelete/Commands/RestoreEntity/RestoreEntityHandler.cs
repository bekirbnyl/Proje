using MediatR;
using Sinema.Domain.Entities;
using Sinema.Domain.Interfaces.Services;

namespace Sinema.Application.Features.Common.SoftDelete.Commands.RestoreEntity;

/// <summary>
/// Handler for restoring soft-deleted entities with validation and audit logging
/// </summary>
public class RestoreEntityHandler : IRequestHandler<RestoreEntityCommand, bool>
{
    private readonly ISoftDeleteService _softDeleteService;
    private readonly IAuditLogger _auditLogger;

    public RestoreEntityHandler(ISoftDeleteService softDeleteService, IAuditLogger auditLogger)
    {
        _softDeleteService = softDeleteService;
        _auditLogger = auditLogger;
    }

    public async Task<bool> Handle(RestoreEntityCommand request, CancellationToken cancellationToken)
    {
        // Determine entity type and perform restore
        var result = request.EntityType.ToLowerInvariant() switch
        {
            "movie" => await _softDeleteService.RestoreAsync<Movie>(request.EntityId, request.UserId, cancellationToken),
            "hall" => await _softDeleteService.RestoreAsync<Hall>(request.EntityId, request.UserId, cancellationToken),
            "seatlayout" => await _softDeleteService.RestoreAsync<SeatLayout>(request.EntityId, request.UserId, cancellationToken),
            "screening" => await _softDeleteService.RestoreAsync<Screening>(request.EntityId, request.UserId, cancellationToken),
            _ => throw new ArgumentException($"Unsupported entity type for restore: {request.EntityType}")
        };

        // If successful, log the restoration
        if (result)
        {
            await _auditLogger.LogRestoreAsync(
                request.EntityType,
                request.EntityId,
                request.UserId,
                cancellationToken: cancellationToken);
        }

        return result;
    }
}
