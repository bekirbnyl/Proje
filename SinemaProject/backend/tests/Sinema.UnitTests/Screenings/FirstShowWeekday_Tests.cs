using Microsoft.EntityFrameworkCore;
using Sinema.Domain.Entities;
using Sinema.Infrastructure.Persistence;
using Sinema.Infrastructure.Services.Scheduling;

namespace Sinema.UnitTests.Screenings;

/// <summary>
/// Unit tests for first show weekday calculation
/// </summary>
public class FirstShowWeekdayTests : IDisposable
{
    private readonly SinemaDbContext _context;
    private readonly ScreeningSchedulingService _schedulingService;

    public FirstShowWeekdayTests()
    {
        var options = new DbContextOptionsBuilder<SinemaDbContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;

        _context = new SinemaDbContext(options);
        _schedulingService = new ScreeningSchedulingService(_context);
    }

    [Fact]
    public async Task IsFirstShowWeekdayAsync_WhenWeekend_ShouldReturnFalse()
    {
        // Arrange
        var hallId = Guid.NewGuid();
        var saturdayDate = GetNextWeekday(DayOfWeek.Saturday);

        // Act
        var isFirstShow = await _schedulingService.IsFirstShowWeekdayAsync(hallId, saturdayDate);

        // Assert
        Assert.False(isFirstShow);
    }

    [Fact]
    public async Task IsFirstShowWeekdayAsync_WhenSunday_ShouldReturnFalse()
    {
        // Arrange
        var hallId = Guid.NewGuid();
        var sundayDate = GetNextWeekday(DayOfWeek.Sunday);

        // Act
        var isFirstShow = await _schedulingService.IsFirstShowWeekdayAsync(hallId, sundayDate);

        // Assert
        Assert.False(isFirstShow);
    }

    [Fact]
    public async Task IsFirstShowWeekdayAsync_WhenFirstWeekdayShow_ShouldReturnTrue()
    {
        // Arrange
        var hallId = Guid.NewGuid();
        var mondayDate = GetNextWeekday(DayOfWeek.Monday).AddHours(10);

        // Act
        var isFirstShow = await _schedulingService.IsFirstShowWeekdayAsync(hallId, mondayDate);

        // Assert
        Assert.True(isFirstShow);
    }

    [Fact]
    public async Task IsFirstShowWeekdayAsync_WhenNotFirstWeekdayShow_ShouldReturnFalse()
    {
        // Arrange
        var hallId = Guid.NewGuid();
        var movieId = Guid.NewGuid();
        var seatLayoutId = Guid.NewGuid();
        var mondayDate = GetNextWeekday(DayOfWeek.Monday);

        // Create earlier screening at 09:00
        var earlierScreening = new Screening
        {
            Id = Guid.NewGuid(),
            HallId = hallId,
            MovieId = movieId,
            SeatLayoutId = seatLayoutId,
            StartAt = mondayDate.AddHours(9),
            DurationMinutes = 120,
            CreatedAt = DateTime.UtcNow
        };

        _context.Screenings.Add(earlierScreening);
        await _context.SaveChangesAsync();

        // Check if 11:00 screening is first show
        var laterShowTime = mondayDate.AddHours(11);

        // Act
        var isFirstShow = await _schedulingService.IsFirstShowWeekdayAsync(hallId, laterShowTime);

        // Assert
        Assert.False(isFirstShow);
    }

    [Fact]
    public async Task IsFirstShowWeekdayAsync_WhenEarlierShowInDifferentHall_ShouldReturnTrue()
    {
        // Arrange
        var hallId1 = Guid.NewGuid();
        var hallId2 = Guid.NewGuid();
        var movieId = Guid.NewGuid();
        var seatLayoutId = Guid.NewGuid();
        var mondayDate = GetNextWeekday(DayOfWeek.Monday);

        // Create earlier screening in different hall at 09:00
        var earlierScreening = new Screening
        {
            Id = Guid.NewGuid(),
            HallId = hallId1,
            MovieId = movieId,
            SeatLayoutId = seatLayoutId,
            StartAt = mondayDate.AddHours(9),
            DurationMinutes = 120,
            CreatedAt = DateTime.UtcNow
        };

        _context.Screenings.Add(earlierScreening);
        await _context.SaveChangesAsync();

        // Check if 10:00 screening in different hall is first show
        var showTime = mondayDate.AddHours(10);

        // Act
        var isFirstShow = await _schedulingService.IsFirstShowWeekdayAsync(hallId2, showTime);

        // Assert
        Assert.True(isFirstShow);
    }

    [Theory]
    [InlineData(DayOfWeek.Monday)]
    [InlineData(DayOfWeek.Tuesday)]
    [InlineData(DayOfWeek.Wednesday)]
    [InlineData(DayOfWeek.Thursday)]
    [InlineData(DayOfWeek.Friday)]
    public async Task IsFirstShowWeekdayAsync_WhenFirstShowOnWeekday_ShouldReturnTrue(DayOfWeek dayOfWeek)
    {
        // Arrange
        var hallId = Guid.NewGuid();
        var weekdayDate = GetNextWeekday(dayOfWeek).AddHours(10);

        // Act
        var isFirstShow = await _schedulingService.IsFirstShowWeekdayAsync(hallId, weekdayDate);

        // Assert
        Assert.True(isFirstShow);
    }

    private static DateTime GetNextWeekday(DayOfWeek targetDay)
    {
        var today = DateTime.Today;
        var daysUntilTarget = ((int)targetDay - (int)today.DayOfWeek + 7) % 7;
        if (daysUntilTarget == 0 && DateTime.Now.Hour > 12) // If today and past noon, get next week
            daysUntilTarget = 7;
        
        return today.AddDays(daysUntilTarget);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
