using Microsoft.EntityFrameworkCore;
using Sinema.Application.Abstractions.Scheduling;
using Sinema.Application.Features.Screenings.Commands.CreateScreening;
using Sinema.Domain.Entities;
using Sinema.Infrastructure.Persistence;
using Sinema.Infrastructure.Services.Scheduling;

namespace Sinema.UnitTests.Screenings;

/// <summary>
/// Unit tests for CreateScreening overlap validation
/// </summary>
public class CreateScreeningOverlapTests : IDisposable
{
    private readonly SinemaDbContext _context;
    private readonly IScreeningSchedulingService _schedulingService;
    private readonly SpecialDayService _specialDayService;

    public CreateScreeningOverlapTests()
    {
        var options = new DbContextOptionsBuilder<SinemaDbContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;

        _context = new SinemaDbContext(options);
        _schedulingService = new ScreeningSchedulingService(_context);
        
        // For unit tests, we'll mock the special day service or create a simple implementation
        // Since we don't have ISettingsRepository mock here, we'll create a minimal test context
    }

    [Fact]
    public async Task HasOverlapAsync_WhenNoExistingScreenings_ShouldReturnFalse()
    {
        // Arrange
        var hallId = Guid.NewGuid();
        var startAt = DateTime.UtcNow.AddHours(2);
        var durationMinutes = 120;

        // Act
        var hasOverlap = await _schedulingService.HasOverlapAsync(hallId, startAt, durationMinutes);

        // Assert
        Assert.False(hasOverlap);
    }

    [Fact]
    public async Task HasOverlapAsync_WhenScreeningsDoNotOverlap_ShouldReturnFalse()
    {
        // Arrange
        var hallId = Guid.NewGuid();
        var movieId = Guid.NewGuid();
        var seatLayoutId = Guid.NewGuid();

        // Create existing screening: 10:00 - 12:00
        var existingScreening = new Screening
        {
            Id = Guid.NewGuid(),
            HallId = hallId,
            MovieId = movieId,
            SeatLayoutId = seatLayoutId,
            StartAt = DateTime.UtcNow.Date.AddHours(10),
            DurationMinutes = 120,
            CreatedAt = DateTime.UtcNow
        };

        _context.Screenings.Add(existingScreening);
        await _context.SaveChangesAsync();

        // New screening: 13:00 - 15:00 (1 hour gap)
        var newStartAt = DateTime.UtcNow.Date.AddHours(13);
        var newDurationMinutes = 120;

        // Act
        var hasOverlap = await _schedulingService.HasOverlapAsync(hallId, newStartAt, newDurationMinutes);

        // Assert
        Assert.False(hasOverlap);
    }

    [Fact]
    public async Task HasOverlapAsync_WhenScreeningsOverlap_ShouldReturnTrue()
    {
        // Arrange
        var hallId = Guid.NewGuid();
        var movieId = Guid.NewGuid();
        var seatLayoutId = Guid.NewGuid();

        // Create existing screening: 10:00 - 12:00
        var existingScreening = new Screening
        {
            Id = Guid.NewGuid(),
            HallId = hallId,
            MovieId = movieId,
            SeatLayoutId = seatLayoutId,
            StartAt = DateTime.UtcNow.Date.AddHours(10),
            DurationMinutes = 120,
            CreatedAt = DateTime.UtcNow
        };

        _context.Screenings.Add(existingScreening);
        await _context.SaveChangesAsync();

        // New screening: 11:00 - 13:00 (overlaps with existing)
        var newStartAt = DateTime.UtcNow.Date.AddHours(11);
        var newDurationMinutes = 120;

        // Act
        var hasOverlap = await _schedulingService.HasOverlapAsync(hallId, newStartAt, newDurationMinutes);

        // Assert
        Assert.True(hasOverlap);
    }

    [Fact]
    public async Task HasOverlapAsync_WhenScreeningsInDifferentHalls_ShouldReturnFalse()
    {
        // Arrange
        var hallId1 = Guid.NewGuid();
        var hallId2 = Guid.NewGuid();
        var movieId = Guid.NewGuid();
        var seatLayoutId = Guid.NewGuid();

        // Create existing screening in hall 1: 10:00 - 12:00
        var existingScreening = new Screening
        {
            Id = Guid.NewGuid(),
            HallId = hallId1,
            MovieId = movieId,
            SeatLayoutId = seatLayoutId,
            StartAt = DateTime.UtcNow.Date.AddHours(10),
            DurationMinutes = 120,
            CreatedAt = DateTime.UtcNow
        };

        _context.Screenings.Add(existingScreening);
        await _context.SaveChangesAsync();

        // New screening in hall 2: 11:00 - 13:00 (same time, different hall)
        var newStartAt = DateTime.UtcNow.Date.AddHours(11);
        var newDurationMinutes = 120;

        // Act
        var hasOverlap = await _schedulingService.HasOverlapAsync(hallId2, newStartAt, newDurationMinutes);

        // Assert
        Assert.False(hasOverlap);
    }

    [Fact]
    public async Task HasOverlapAsync_WhenExcludingScreening_ShouldNotCheckExcludedScreening()
    {
        // Arrange
        var hallId = Guid.NewGuid();
        var movieId = Guid.NewGuid();
        var seatLayoutId = Guid.NewGuid();

        // Create existing screening: 10:00 - 12:00
        var existingScreening = new Screening
        {
            Id = Guid.NewGuid(),
            HallId = hallId,
            MovieId = movieId,
            SeatLayoutId = seatLayoutId,
            StartAt = DateTime.UtcNow.Date.AddHours(10),
            DurationMinutes = 120,
            CreatedAt = DateTime.UtcNow
        };

        _context.Screenings.Add(existingScreening);
        await _context.SaveChangesAsync();

        // Check overlap with same time and duration, but exclude the existing screening
        var checkStartAt = DateTime.UtcNow.Date.AddHours(10);
        var checkDurationMinutes = 120;

        // Act - should not find overlap because we're excluding the existing screening
        var hasOverlap = await _schedulingService.HasOverlapAsync(
            hallId, 
            checkStartAt, 
            checkDurationMinutes, 
            existingScreening.Id);

        // Assert
        Assert.False(hasOverlap);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
