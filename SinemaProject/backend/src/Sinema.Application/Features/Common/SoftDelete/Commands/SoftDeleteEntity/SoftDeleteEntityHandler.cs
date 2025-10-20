using MediatR;
using Sinema.Domain.Entities;
using Sinema.Domain.Interfaces.Services;

namespace Sinema.Application.Features.Common.SoftDelete.Commands.SoftDeleteEntity;

/// <summary>
/// Handler for soft-deleting entities with validation and audit logging
/// </summary>
public class SoftDeleteEntityHandler : IRequestHandler<SoftDeleteEntityCommand, bool>
{
    private readonly ISoftDeleteService _softDeleteService;
    private readonly IAuditLogger _auditLogger;

    public SoftDeleteEntityHandler(ISoftDeleteService softDeleteService, IAuditLogger auditLogger)
    {
        _softDeleteService = softDeleteService;
        _auditLogger = auditLogger;
    }

    public async Task<bool> Handle(SoftDeleteEntityCommand request, CancellationToken cancellationToken)
    {
        // Determine entity type and perform soft delete
        var result = request.EntityType.ToLowerInvariant() switch
        {
            "movie" => await _softDeleteService.SoftDeleteAsync<Movie>(request.EntityId, request.Reason, request.UserId, cancellationToken),
            "hall" => await _softDeleteService.SoftDeleteAsync<Hall>(request.EntityId, request.Reason, request.UserId, cancellationToken),
            "seatlayout" => await _softDeleteService.SoftDeleteAsync<SeatLayout>(request.EntityId, request.Reason, request.UserId, cancellationToken),
            "screening" => await _softDeleteService.SoftDeleteAsync<Screening>(request.EntityId, request.Reason, request.UserId, cancellationToken),
            _ => throw new ArgumentException($"Unsupported entity type for soft delete: {request.EntityType}")
        };

        // If successful, log the deletion (this might be redundant if the interceptor already logs it)
        if (result)
        {
            await _auditLogger.LogDeletionAsync(
                request.EntityType,
                request.EntityId,
                request.Reason,
                request.UserId,
                request.ApprovedBy,
                cancellationToken: cancellationToken);
        }

        return result;
    }
}
