using MediatR;
using Sinema.Application.DTOs.Movies;
using Sinema.Domain.Interfaces.Repositories;

namespace Sinema.Application.Features.Movies.Queries.GetMovies;

/// <summary>
/// Handler for GetMoviesQuery
/// </summary>
public class GetMoviesHandler : IRequestHandler<GetMoviesQuery, IEnumerable<MovieDto>>
{
    private readonly IMovieRepository _movieRepository;

    public GetMoviesHandler(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;
    }

    public async Task<IEnumerable<MovieDto>> Handle(GetMoviesQuery request, CancellationToken cancellationToken)
    {
        var movies = await _movieRepository.GetAllMoviesAsync(request.Active, cancellationToken);

        return movies.Select(m => new MovieDto
        {
            Id = m.Id,
            Title = m.Title,
            DurationMinutes = m.DurationMinutes,
            IsActive = m.IsActive,
            CreatedAt = m.CreatedAt
        });
    }
}
