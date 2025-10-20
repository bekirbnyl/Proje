using MediatR;
using Sinema.Application.DTOs.Screenings;

namespace Sinema.Application.Features.Screenings.Queries.GetScreeningsByDate;

/// <summary>
/// Query to get screenings filtered by date, hall, and/or movie
/// </summary>
public record GetScreeningsByDateQuery(
    DateTime? Date = null,
    Guid? HallId = null,
    Guid? MovieId = null
) : IRequest<IEnumerable<ScreeningListItemDto>>;
