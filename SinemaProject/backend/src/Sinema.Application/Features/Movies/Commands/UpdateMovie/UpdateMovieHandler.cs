using MediatR;
using Sinema.Application.DTOs.Movies;
using Sinema.Domain.Interfaces.Repositories;

namespace Sinema.Application.Features.Movies.Commands.UpdateMovie;

/// <summary>
/// Handler for UpdateMovieCommand
/// </summary>
public class UpdateMovieHandler : IRequestHandler<UpdateMovieCommand, MovieDto>
{
    private readonly IMovieRepository _movieRepository;

    public UpdateMovieHandler(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;
    }

    public async Task<MovieDto> Handle(UpdateMovieCommand request, CancellationToken cancellationToken)
    {
        var movie = await _movieRepository.GetByIdAsync(request.Id, cancellationToken);

        if (movie == null)
        {
            throw new InvalidOperationException($"Movie with ID '{request.Id}' not found.");
        }

        // Check if another movie with same title exists (excluding current movie)
        var existingMovie = await _movieRepository.GetByTitleAsync(request.Title, cancellationToken);

        if (existingMovie != null && existingMovie.Id != request.Id)
        {
            throw new InvalidOperationException($"A movie with title '{request.Title}' already exists.");
        }

        movie.Title = request.Title;
        movie.DurationMinutes = request.DurationMinutes;
        movie.IsActive = request.IsActive;

        await _movieRepository.UpdateAsync(movie, cancellationToken);

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
