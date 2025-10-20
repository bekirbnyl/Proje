using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sinema.Application.DTOs.Screenings;
using Sinema.Domain.Entities;
using Sinema.Infrastructure.Persistence;

namespace Sinema.IntegrationTests.Api;

/// <summary>
/// Integration tests for Screenings CRUD API endpoints
/// </summary>
public class ScreeningsCrudTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly SinemaDbContext _context;

    public ScreeningsCrudTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();

        // Get the DbContext from the DI container
        var scope = _factory.Services.CreateScope();
        _context = scope.ServiceProvider.GetRequiredService<SinemaDbContext>();
        
        // Ensure database is created and clean
        _context.Database.EnsureCreated();
        CleanDatabase();
        SeedTestData();
    }

    [Fact]
    public async Task GetScreenings_ShouldReturnAllScreenings()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/screenings");

        // Assert
        response.EnsureSuccessStatusCode();
        var screenings = await response.Content.ReadFromJsonAsync<List<ScreeningListItemDto>>();
        Assert.NotNull(screenings);
        Assert.True(screenings.Count > 0);
    }

    [Fact]
    public async Task GetScreenings_WithDateFilter_ShouldReturnFilteredScreenings()
    {
        // Arrange
        var filterDate = DateTime.Today.AddDays(1);

        // Act
        var response = await _client.GetAsync($"/api/v1/screenings?date={filterDate:yyyy-MM-dd}");

        // Assert
        response.EnsureSuccessStatusCode();
        var screenings = await response.Content.ReadFromJsonAsync<List<ScreeningListItemDto>>();
        Assert.NotNull(screenings);
        
        foreach (var screening in screenings)
        {
            Assert.Equal(filterDate.Date, screening.StartAt.Date);
        }
    }

    [Fact]
    public async Task GetScreeningById_WhenExists_ShouldReturnScreening()
    {
        // Arrange
        var existingScreening = _context.Screenings.First();

        // Act
        var response = await _client.GetAsync($"/api/v1/screenings/{existingScreening.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        var screening = await response.Content.ReadFromJsonAsync<ScreeningDto>();
        Assert.NotNull(screening);
        Assert.Equal(existingScreening.Id, screening.Id);
    }

    [Fact]
    public async Task GetScreeningById_WhenNotExists_ShouldReturn404()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/v1/screenings/{nonExistentId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateScreening_WithValidData_ShouldReturn201()
    {
        // Arrange
        var movie = _context.Movies.First();
        var hall = _context.Halls.First();

        var request = new CreateScreeningRequest
        {
            MovieId = movie.Id,
            HallId = hall.Id,
            StartAt = DateTime.UtcNow.AddDays(2).AddHours(10),
            DurationMinutes = 120
        };

        // Note: In real integration tests, you would need to authenticate
        // For this example, we'll assume authentication is handled

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/screenings", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var screening = await response.Content.ReadFromJsonAsync<ScreeningDto>();
        Assert.NotNull(screening);
        Assert.Equal(request.MovieId, screening.MovieId);
        Assert.Equal(request.HallId, screening.HallId);
    }

    [Fact]
    public async Task CreateScreening_WithOverlappingTime_ShouldReturn409()
    {
        // Arrange
        var existingScreening = _context.Screenings.First();

        var request = new CreateScreeningRequest
        {
            MovieId = existingScreening.MovieId,
            HallId = existingScreening.HallId,
            StartAt = existingScreening.StartAt.AddMinutes(30), // Overlaps with existing
            DurationMinutes = 120
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/screenings", request);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CreateScreening_WithPastTime_ShouldReturn400()
    {
        // Arrange
        var movie = _context.Movies.First();
        var hall = _context.Halls.First();

        var request = new CreateScreeningRequest
        {
            MovieId = movie.Id,
            HallId = hall.Id,
            StartAt = DateTime.UtcNow.AddHours(-1), // Past time
            DurationMinutes = 120
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/screenings", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateScreening_WithInactiveMovie_ShouldReturn400()
    {
        // Arrange
        var inactiveMovie = _context.Movies.First(m => !m.IsActive);
        var hall = _context.Halls.First();

        var request = new CreateScreeningRequest
        {
            MovieId = inactiveMovie.Id,
            HallId = hall.Id,
            StartAt = DateTime.UtcNow.AddDays(1).AddHours(10),
            DurationMinutes = 120
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/screenings", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateScreening_WithValidData_ShouldReturn200()
    {
        // Arrange
        var existingScreening = _context.Screenings.First();
        var newMovie = _context.Movies.First(m => m.Id != existingScreening.MovieId);

        var request = new UpdateScreeningRequest
        {
            MovieId = newMovie.Id,
            HallId = existingScreening.HallId,
            StartAt = DateTime.UtcNow.AddDays(3).AddHours(14),
            DurationMinutes = 150
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1/screenings/{existingScreening.Id}", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var screening = await response.Content.ReadFromJsonAsync<ScreeningDto>();
        Assert.NotNull(screening);
        Assert.Equal(request.MovieId, screening.MovieId);
        Assert.Equal(request.DurationMinutes, screening.DurationMinutes);
    }

    [Fact]
    public async Task DeleteScreening_WithValidId_ShouldReturn204()
    {
        // Arrange
        var futureScreening = _context.Screenings.First(s => s.StartAt > DateTime.UtcNow);
        var reason = "Test deletion";

        // Act
        var response = await _client.DeleteAsync($"/api/v1/screenings/{futureScreening.Id}?reason={reason}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        
        // Verify screening is deleted
        var deletedScreening = await _context.Screenings.FindAsync(futureScreening.Id);
        Assert.Null(deletedScreening);
    }

    [Fact]
    public async Task DeleteScreening_WithoutReason_ShouldReturn400()
    {
        // Arrange
        var futureScreening = _context.Screenings.First(s => s.StartAt > DateTime.UtcNow);

        // Act
        var response = await _client.DeleteAsync($"/api/v1/screenings/{futureScreening.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private void CleanDatabase()
    {
        _context.Screenings.RemoveRange(_context.Screenings);
        _context.Movies.RemoveRange(_context.Movies);
        _context.Halls.RemoveRange(_context.Halls);
        _context.Cinemas.RemoveRange(_context.Cinemas);
        _context.SeatLayouts.RemoveRange(_context.SeatLayouts);
        _context.DeletionLogs.RemoveRange(_context.DeletionLogs);
        _context.SaveChanges();
    }

    private void SeedTestData()
    {
        // Create cinema
        var cinema = new Cinema
        {
            Id = Guid.NewGuid(),
            Name = "Test Cinema",
            CreatedAt = DateTime.UtcNow
        };
        _context.Cinemas.Add(cinema);

        // Create hall
        var hall = new Hall
        {
            Id = Guid.NewGuid(),
            CinemaId = cinema.Id,
            Name = "Test Hall 1",
            CreatedAt = DateTime.UtcNow
        };
        _context.Halls.Add(hall);

        // Create seat layout
        var seatLayout = new SeatLayout
        {
            Id = Guid.NewGuid(),
            HallId = hall.Id,
            Version = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _context.SeatLayouts.Add(seatLayout);

        // Create movies
        var activeMovie = new Movie
        {
            Id = Guid.NewGuid(),
            Title = "Active Movie",
            DurationMinutes = 120,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var inactiveMovie = new Movie
        {
            Id = Guid.NewGuid(),
            Title = "Inactive Movie",
            DurationMinutes = 110,
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Movies.AddRange(activeMovie, inactiveMovie);

        // Create screenings
        var screening1 = new Screening
        {
            Id = Guid.NewGuid(),
            MovieId = activeMovie.Id,
            HallId = hall.Id,
            SeatLayoutId = seatLayout.Id,
            StartAt = DateTime.UtcNow.AddDays(1).AddHours(10),
            DurationMinutes = 120,
            IsFirstShowWeekday = true,
            IsSpecialDay = false,
            CreatedAt = DateTime.UtcNow
        };

        var screening2 = new Screening
        {
            Id = Guid.NewGuid(),
            MovieId = activeMovie.Id,
            HallId = hall.Id,
            SeatLayoutId = seatLayout.Id,
            StartAt = DateTime.UtcNow.AddDays(1).AddHours(15),
            DurationMinutes = 120,
            IsFirstShowWeekday = false,
            IsSpecialDay = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Screenings.AddRange(screening1, screening2);
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
        _client.Dispose();
    }
}
