using MediatR;
using Sinema.Application.DTOs.Movies;

namespace Sinema.Application.Features.Movies.Commands.CreateMovie;

/// <summary>
/// Command to create a new movie
/// </summary>
public record CreateMovieCommand(
    string Title,
    int DurationMinutes,
    bool IsActive = true
) : IRequest<MovieDto>;
