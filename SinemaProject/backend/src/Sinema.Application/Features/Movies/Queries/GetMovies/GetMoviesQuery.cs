using MediatR;
using Sinema.Application.DTOs.Movies;

namespace Sinema.Application.Features.Movies.Queries.GetMovies;

/// <summary>
/// Query to get movies with optional filtering by active status
/// </summary>
public record GetMoviesQuery(
    bool? Active = null
) : IRequest<IEnumerable<MovieDto>>;
