using MediatR;
using Sinema.Domain.Entities;
using Sinema.Domain.Interfaces.Repositories;

namespace Sinema.Application.Features.Movies.Commands.DeleteMovie;

/// <summary>
/// Handler for DeleteMovieCommand
/// </summary>
public class DeleteMovieHandler : IRequestHandler<DeleteMovieCommand>
{
    private readonly IMovieRepository _movieRepository;
    private readonly IDeletionLogRepository _deletionLogRepository;

    public DeleteMovieHandler(IMovieRepository movieRepository, IDeletionLogRepository deletionLogRepository)
    {
        _movieRepository = movieRepository;
        _deletionLogRepository = deletionLogRepository;
    }

    public async Task Handle(DeleteMovieCommand request, CancellationToken cancellationToken)
    {
        var movie = await _movieRepository.GetByIdAsync(request.Id, cancellationToken);

        if (movie == null)
        {
            throw new InvalidOperationException($"Movie with ID '{request.Id}' not found.");
        }

        // Check if movie has future screenings
        var hasFutureScreenings = await _movieRepository.HasFutureScreeningsAsync(request.Id, cancellationToken);

        if (hasFutureScreenings)
        {
            throw new InvalidOperationException("Cannot delete movie with future screenings. Please cancel or reschedule all future screenings first.");
        }

        // Create deletion log entry
        var deletionLog = new DeletionLog
        {
            Id = Guid.NewGuid(),
            EntityName = nameof(Movie),
            EntityId = movie.Id,
            Reason = request.Reason,
            DeletedAt = DateTime.UtcNow
        };

        await _deletionLogRepository.AddAsync(deletionLog, cancellationToken);
        await _movieRepository.DeleteAsync(movie, cancellationToken);
    }
}
