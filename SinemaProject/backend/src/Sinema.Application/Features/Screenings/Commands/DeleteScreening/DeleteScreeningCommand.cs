using MediatR;

namespace Sinema.Application.Features.Screenings.Commands.DeleteScreening;

/// <summary>
/// Command to delete a screening
/// </summary>
public record DeleteScreeningCommand(
    Guid Id,
    string Reason
) : IRequest;
