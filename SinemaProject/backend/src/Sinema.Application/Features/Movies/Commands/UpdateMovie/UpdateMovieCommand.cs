using MediatR;
using Sinema.Application.DTOs.Movies;

namespace Sinema.Application.Features.Movies.Commands.UpdateMovie;

/// <summary>
/// Command to update an existing movie
/// </summary>
public record UpdateMovieCommand(
    Guid Id,
    string Title,
    int DurationMinutes,
    bool IsActive
) : IRequest<MovieDto>;
