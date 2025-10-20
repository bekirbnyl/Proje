using MediatR;
using Sinema.Application.DTOs.Screenings;
using Sinema.Domain.Interfaces.Repositories;

namespace Sinema.Application.Features.Screenings.Queries.GetScreeningsByDate;

/// <summary>
/// Handler for GetScreeningsByDateQuery
/// </summary>
public class GetScreeningsByDateHandler : IRequestHandler<GetScreeningsByDateQuery, IEnumerable<ScreeningListItemDto>>
{
    private readonly IScreeningRepository _screeningRepository;

    public GetScreeningsByDateHandler(IScreeningRepository screeningRepository)
    {
        _screeningRepository = screeningRepository;
    }

    public async Task<IEnumerable<ScreeningListItemDto>> Handle(GetScreeningsByDateQuery request, CancellationToken cancellationToken)
    {
        var screenings = await _screeningRepository.GetFilteredScreeningsAsync(
            request.Date, 
            request.HallId, 
            request.MovieId, 
            cancellationToken);

        return screenings.Select(s => new ScreeningListItemDto
        {
            Id = s.Id,
            MovieId = s.MovieId,
            HallId = s.HallId,
            MovieTitle = s.Movie.Title,
            HallName = s.Hall.Name,
            CinemaName = s.Hall.Cinema.Name,
            StartAt = s.StartAt,
            DurationMinutes = s.DurationMinutes,
            IsFirstShowWeekday = s.IsFirstShowWeekday,
            IsSpecialDay = s.IsSpecialDay
        });
    }
}
