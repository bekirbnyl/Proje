using MediatR;
using Sinema.Application.Abstractions.Scheduling;
using Sinema.Application.DTOs.Screenings;
using Sinema.Domain.Entities;
using Sinema.Domain.Interfaces.Repositories;

namespace Sinema.Application.Features.Screenings.Commands.CreateScreening;

/// <summary>
/// Handler for CreateScreeningCommand
/// </summary>
public class CreateScreeningHandler : IRequestHandler<CreateScreeningCommand, ScreeningDto>
{
    private readonly IMovieRepository _movieRepository;
    private readonly IHallRepository _hallRepository;
    private readonly ISeatLayoutRepository _seatLayoutRepository;
    private readonly IScreeningRepository _screeningRepository;
    private readonly IScreeningSchedulingService _schedulingService;
    private readonly ISpecialDayService _specialDayService;

    public CreateScreeningHandler(
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

    public async Task<ScreeningDto> Handle(CreateScreeningCommand request, CancellationToken cancellationToken)
    {
        // Validate that the start time is not in the past (with 5 minute tolerance)
        if (request.StartAt < DateTime.UtcNow.AddMinutes(-5))
        {
            throw new InvalidOperationException("Cannot create screenings in the past.");
        }

        // Validate movie exists and is active
        var movie = await _movieRepository.GetByIdAsync(request.MovieId, cancellationToken);

        if (movie == null)
        {
            throw new InvalidOperationException($"Movie with ID '{request.MovieId}' not found.");
        }

        if (!movie.IsActive)
        {
            throw new InvalidOperationException("Cannot create screenings for inactive movies.");
        }

        // Validate hall exists
        var hall = await _hallRepository.GetByIdAsync(request.HallId, cancellationToken);

        if (hall == null)
        {
            throw new InvalidOperationException($"Hall with ID '{request.HallId}' not found.");
        }

        // Determine duration (use provided or movie's duration or default 120)
        var durationMinutes = request.DurationMinutes ?? movie.DurationMinutes;
        if (durationMinutes <= 0)
        {
            durationMinutes = 120; // Default duration
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

        // Check for overlapping screenings
        var hasOverlap = await _schedulingService.HasOverlapAsync(
            request.HallId, 
            request.StartAt, 
            durationMinutes, 
            cancellationToken: cancellationToken);

        if (hasOverlap)
        {
            throw new InvalidOperationException("The screening time conflicts with an existing screening in the same hall.");
        }

        // Calculate scheduling flags
        var isFirstShowWeekday = await _schedulingService.IsFirstShowWeekdayAsync(
            request.HallId, 
            request.StartAt, 
            cancellationToken);

        var isSpecialDay = await _specialDayService.IsHalkGunuAsync(
            request.StartAt, 
            cancellationToken);

        // Create the screening
        var screening = new Screening
        {
            Id = Guid.NewGuid(),
            MovieId = request.MovieId,
            HallId = request.HallId,
            SeatLayoutId = seatLayoutId.Value,
            StartAt = request.StartAt,
            DurationMinutes = durationMinutes,
            IsFirstShowWeekday = isFirstShowWeekday,
            IsSpecialDay = isSpecialDay,
            CreatedAt = DateTime.UtcNow
        };

        await _screeningRepository.AddAsync(screening, cancellationToken);

        // Return DTO with navigation properties
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
