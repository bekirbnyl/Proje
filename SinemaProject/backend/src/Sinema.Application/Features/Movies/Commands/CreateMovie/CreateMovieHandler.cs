using MediatR;
using Sinema.Application.DTOs.Movies;
using Sinema.Domain.Entities;
using Sinema.Domain.Interfaces.Repositories;

namespace Sinema.Application.Features.Movies.Commands.CreateMovie;

/// <summary>
/// Handler for CreateMovieCommand
/// </summary>
public class CreateMovieHandler : IRequestHandler<CreateMovieCommand, MovieDto>
{
    private readonly IMovieRepository _movieRepository;

    public CreateMovieHandler(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;
    }

    public async Task<MovieDto> Handle(CreateMovieCommand request, CancellationToken cancellationToken)
    {
        // Check if movie with same title already exists
        var existingMovie = await _movieRepository.GetByTitleAsync(request.Title, cancellationToken);

        if (existingMovie != null)
        {
            throw new InvalidOperationException($"A movie with title '{request.Title}' already exists.");
        }

        var movie = new Movie
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            DurationMinutes = request.DurationMinutes,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        await _movieRepository.AddAsync(movie, cancellationToken);

        return new MovieDto
        {
            Id = movie.Id,
            Title = movie.Title,
            DurationMinutes = movie.DurationMinutes,
            IsActive = movie.IsActive,
            CreatedAt = movie.CreatedAt
        };
    }
}
