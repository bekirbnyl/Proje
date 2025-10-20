using MediatR;
using Sinema.Application.DTOs.Screenings;

namespace Sinema.Application.Features.Screenings.Commands.UpdateScreening;

/// <summary>
/// Command to update an existing screening
/// </summary>
public record UpdateScreeningCommand(
    Guid Id,
    Guid MovieId,
    Guid HallId,
    DateTime StartAt,
    int DurationMinutes,
    Guid? SeatLayoutId = null
) : IRequest<ScreeningDto>;
