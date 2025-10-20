using MediatR;
using Sinema.Application.DTOs.Screenings;
using Sinema.Domain.Interfaces.Repositories;

namespace Sinema.Application.Features.Screenings.Queries.GetScreeningById;

/// <summary>
/// Handler for GetScreeningByIdQuery
/// </summary>
public class GetScreeningByIdHandler : IRequestHandler<GetScreeningByIdQuery, ScreeningDto?>
{
    private readonly IScreeningRepository _screeningRepository;

    public GetScreeningByIdHandler(IScreeningRepository screeningRepository)
    {
        _screeningRepository = screeningRepository;
    }

    public async Task<ScreeningDto?> Handle(GetScreeningByIdQuery request, CancellationToken cancellationToken)
    {
        var screening = await _screeningRepository.GetByIdWithDetailsAsync(request.Id, cancellationToken);

        if (screening == null)
            return null;

        return new ScreeningDto
        {
            Id = screening.Id,
            MovieId = screening.MovieId,
            HallId = screening.HallId,
            SeatLayoutId = screening.SeatLayoutId,
            StartAt = screening.StartAt,
            DurationMinutes = screening.DurationMinutes,
            IsFirstShowWeekday = screening.IsFirstShowWeekday,
            IsSpecialDay = screening.IsSpecialDay,
            CreatedAt = screening.CreatedAt,
            MovieTitle = screening.Movie.Title,
            HallName = screening.Hall.Name
        };
    }
}
