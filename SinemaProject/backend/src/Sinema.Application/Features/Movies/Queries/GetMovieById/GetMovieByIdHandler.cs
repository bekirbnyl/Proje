using MediatR;
using Sinema.Application.DTOs.Movies;
using Sinema.Domain.Interfaces.Repositories;

namespace Sinema.Application.Features.Movies.Queries.GetMovieById;

/// <summary>
/// Handler for GetMovieByIdQuery
/// </summary>
public class GetMovieByIdHandler : IRequestHandler<GetMovieByIdQuery, MovieDto?>
{
    private readonly IMovieRepository _movieRepository;

    public GetMovieByIdHandler(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;
    }

    public async Task<MovieDto?> Handle(GetMovieByIdQuery request, CancellationToken cancellationToken)
    {
        var movie = await _movieRepository.GetByIdAsync(request.Id, cancellationToken);

        if (movie == null)
            return null;

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
