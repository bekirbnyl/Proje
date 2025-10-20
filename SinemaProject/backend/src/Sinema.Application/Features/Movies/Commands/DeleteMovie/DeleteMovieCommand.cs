using MediatR;

namespace Sinema.Application.Features.Movies.Commands.DeleteMovie;

/// <summary>
/// Command to delete a movie
/// </summary>
public record DeleteMovieCommand(
    Guid Id,
    string Reason
) : IRequest;
