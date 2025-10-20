using MediatR;
using Sinema.Application.DTOs.Movies;

namespace Sinema.Application.Features.Movies.Queries.GetMovieById;

/// <summary>
/// Query to get a movie by its ID
/// </summary>
public record GetMovieByIdQuery(
    Guid Id
) : IRequest<MovieDto?>;
