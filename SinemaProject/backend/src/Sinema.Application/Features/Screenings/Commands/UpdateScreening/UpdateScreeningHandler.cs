using MediatR;
using Sinema.Application.Abstractions.Scheduling;
using Sinema.Application.DTOs.Screenings;
using Sinema.Domain.Interfaces.Repositories;

namespace Sinema.Application.Features.Screenings.Commands.UpdateScreening;

/// <summary>
/// Handler for UpdateScreeningCommand
/// </summary>
public class UpdateScreeningHandler : IRequestHandler<UpdateScreeningCommand, ScreeningDto>
{
    private readonly IMovieRepository _movieRepository;
    private readonly IHallRepository _hallRepository;
    private readonly ISeatLayoutRepository _seatLayoutRepository;
    private readonly IScreeningRepository _screeningRepository;
    private readonly IScreeningSchedulingService _schedulingService;
    private readonly ISpecialDayService _specialDayService;

    public UpdateScreeningHandler(
        IMovieRepository movieRepository,
        IHallRepository hallRepository,
        ISeatLayoutRepository seatLayoutRepository,
        IScreeningRepository screeningRepository,
        IScreeningSchedulingService schedulingService,
        ISpecialDayService specialDayService)
    {
        _movieRepository = movieRepository;
        _hallRepository = hallRepository;
        _seatLayoutRepository = seatLayoutRepository;
        _screeningRepository = screeningRepository;
        _schedulingService = schedulingService;
        _specialDayService = specialDayService;
    }

    public async Task<ScreeningDto> Handle(UpdateScreeningCommand request, CancellationToken cancellationToken)
    {
        // Find the existing screening
        var screening = await _screeningRepository.GetByIdWithDetailsAsync(request.Id, cancellationToken);

        if (screening == null)
        {
            throw new InvalidOperationException($"Screening with ID '{request.Id}' not found.");
        }

        // Validate that the start time is not in the past (with 5 minute tolerance)
        if (request.StartAt < DateTime.UtcNow.AddMinutes(-5))
        {
            throw new InvalidOperationException("Cannot update screening to a time in the past.");
        }

        // Validate movie exists and is active
        var movie = await _movieRepository.GetByIdAsync(request.MovieId, cancellationToken);

        if (movie == null)
        {
            throw new InvalidOperationException($"Movie with ID '{request.MovieId}' not found.");
        }

        if (!movie.IsActive)
        {
            throw new InvalidOperationException("Cannot assign inactive movies to screenings.");
        }

        // Validate hall exists
        var hall = await _hallRepository.GetByIdAsync(request.HallId, cancellationToken);

        if (hall == null)
        {
            throw new InvalidOperationException($"Hall with ID '{request.HallId}' not found.");
        }

        // Validate duration
        if (request.DurationMinutes <= 0)
        {
            throw new InvalidOperationException("Duration must be greater than 0 minutes.");
        }

        // Determine seat layout (use provided or hall's active layout)
        var seatLayoutId = request.SeatLayoutId;
        if (!seatLayoutId.HasValue)
        {
            var activeSeatLayout = await _seatLayoutRepository.GetActiveByHallIdAsync(request.HallId, cancellationToken);

            if (activeSeatLayout == null)
            {
                throw new InvalidOperationException($"No active seat layout found for hall '{request.HallId}'.");
            }

            seatLayoutId = activeSeatLayout.Id;
        }
        else
        {
            // Validate that the provided seat layout belongs to the hall
            var seatLayout = await _seatLayoutRepository.GetByIdAsync(seatLayoutId.Value, cancellationToken);

            if (seatLayout == null || seatLayout.HallId != request.HallId)
            {
                throw new InvalidOperationException($"Seat layout '{seatLayoutId}' does not belong to hall '{request.HallId}'.");
            }
        }

        // Check for overlapping screenings (exclude current screening)
        var hasOverlap = await _schedulingService.HasOverlapAsync(
            request.HallId, 
            request.StartAt, 
            request.DurationMinutes, 
            request.Id, 
            cancellationToken);

        if (hasOverlap)
        {
            throw new InvalidOperationException("The screening time conflicts with an existing screening in the same hall.");
        }

        // Calculate scheduling flags (recalculate if time changed)
        var isFirstShowWeekday = await _schedulingService.IsFirstShowWeekdayAsync(
            request.HallId, 
            request.StartAt, 
            cancellationToken);

        var isSpecialDay = await _specialDayService.IsHalkGunuAsync(
            request.StartAt, 
            cancellationToken);

        // Update the screening
        screening.MovieId = request.MovieId;
        screening.HallId = request.HallId;
        screening.SeatLayoutId = seatLayoutId.Value;
        screening.StartAt = request.StartAt;
        screening.DurationMinutes = request.DurationMinutes;
        screening.IsFirstShowWeekday = isFirstShowWeekday;
        screening.IsSpecialDay = isSpecialDay;

        await _screeningRepository.UpdateAsync(screening, cancellationToken);

        // Return DTO with updated navigation properties
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
            MovieTitle = movie.Title,
            HallName = hall.Name
        };
    }
}
