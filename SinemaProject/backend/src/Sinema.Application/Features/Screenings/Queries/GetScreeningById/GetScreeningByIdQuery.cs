using MediatR;
using Sinema.Application.DTOs.Screenings;

namespace Sinema.Application.Features.Screenings.Queries.GetScreeningById;

/// <summary>
/// Query to get a screening by its ID
/// </summary>
public record GetScreeningByIdQuery(
    Guid Id
) : IRequest<ScreeningDto?>;
