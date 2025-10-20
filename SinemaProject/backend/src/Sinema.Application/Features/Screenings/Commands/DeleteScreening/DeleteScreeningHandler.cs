using MediatR;
using Sinema.Domain.Entities;
using Sinema.Domain.Interfaces.Repositories;

namespace Sinema.Application.Features.Screenings.Commands.DeleteScreening;

/// <summary>
/// Handler for DeleteScreeningCommand
/// </summary>
public class DeleteScreeningHandler : IRequestHandler<DeleteScreeningCommand>
{
    private readonly IScreeningRepository _screeningRepository;
    private readonly IDeletionLogRepository _deletionLogRepository;

    public DeleteScreeningHandler(IScreeningRepository screeningRepository, IDeletionLogRepository deletionLogRepository)
    {
        _screeningRepository = screeningRepository;
        _deletionLogRepository = deletionLogRepository;
    }

    public async Task Handle(DeleteScreeningCommand request, CancellationToken cancellationToken)
    {
        var screening = await _screeningRepository.GetByIdAsync(request.Id, cancellationToken);

        if (screening == null)
        {
            throw new InvalidOperationException($"Screening with ID '{request.Id}' not found.");
        }

        // Only allow deletion of future screenings for audit purposes
        if (screening.StartAt <= DateTime.UtcNow)
        {
            throw new InvalidOperationException("Cannot delete past or currently running screenings for audit purposes.");
        }

        // Create deletion log entry
        var deletionLog = new DeletionLog
        {
            Id = Guid.NewGuid(),
            EntityName = nameof(Screening),
            EntityId = screening.Id,
            Reason = request.Reason,
            DeletedAt = DateTime.UtcNow
        };

        await _deletionLogRepository.AddAsync(deletionLog, cancellationToken);
        await _screeningRepository.DeleteAsync(screening, cancellationToken);
    }
}
