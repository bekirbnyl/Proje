using MediatR;
using Sinema.Application.DTOs.Screenings;

namespace Sinema.Application.Features.Screenings.Commands.CreateScreening;

/// <summary>
/// Command to create a new screening
/// </summary>
public record CreateScreeningCommand(
    Guid MovieId,
    Guid HallId,
    DateTime StartAt,
    int? DurationMinutes = null,
    Guid? SeatLayoutId = null
) : IRequest<ScreeningDto>;
