using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sinema.Application.DTOs.Seating;
using Sinema.Domain.Entities;
using Sinema.Domain.Enums;
using Sinema.Infrastructure.Persistence;
using System.Net.Http.Json;
using Xunit;

namespace Sinema.IntegrationTests.Api;

/// <summary>
/// Integration tests for screening seat status endpoints
/// </summary>
public class ScreeningsSeats_Endpoint_Tests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ScreeningsSeats_Endpoint_Tests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetSeatStatuses_ShouldReturnCorrectSeatGrid_WhenScreeningExists()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SinemaDbContext>();

        // Clear existing data
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        // Create test data
        var cinema = new Cinema
        {
            Id = Guid.NewGuid(),
            Name = "Test Cinema",
            CreatedAt = DateTime.UtcNow
        };

        var hall = new Hall
        {
            Id = Guid.NewGuid(),
            CinemaId = cinema.Id,
            Name = "Test Hall",
            CreatedAt = DateTime.UtcNow
        };

        var seatLayout = new SeatLayout
        {
            Id = Guid.NewGuid(),
            HallId = hall.Id,
            Version = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Create 2x5 seat layout
        var seats = new List<Seat>();
        for (int row = 1; row <= 2; row++)
        {
            for (int col = 1; col <= 5; col++)
            {
                seats.Add(new Seat
                {
                    Id = Guid.NewGuid(),
                    SeatLayoutId = seatLayout.Id,
                    Row = row,
                    Col = col,
                    Label = $"{(char)('A' + row - 1)}{col}"
                });
            }
        }

        var movie = new Movie
        {
            Id = Guid.NewGuid(),
            Title = "Test Movie",
            DurationMinutes = 120,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var screening = new Screening
        {
            Id = Guid.NewGuid(),
            MovieId = movie.Id,
            HallId = hall.Id,
            SeatLayoutId = seatLayout.Id,
            StartAt = DateTime.UtcNow.AddHours(2),
            DurationMinutes = 120,
            CreatedAt = DateTime.UtcNow
        };

        // Sell one ticket (A1 should be "sold")
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            ScreeningId = screening.Id,
            SeatId = seats.First(s => s.Label == "A1").Id,
            Type = TicketType.Full,
            Channel = TicketChannel.Web,
            Price = 25.00m,
            SoldAt = DateTime.UtcNow
        };

        // Reserve one seat (A2 should be "reserved")
        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            ScreeningId = screening.Id,
            SeatId = seats.First(s => s.Label == "A2").Id,
            Status = ReservationStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30),
            CreatedAt = DateTime.UtcNow
        };

        context.Cinemas.Add(cinema);
        context.Halls.Add(hall);
        context.SeatLayouts.Add(seatLayout);
        context.Seats.AddRange(seats);
        context.Movies.Add(movie);
        context.Screenings.Add(screening);
        context.Tickets.Add(ticket);
        context.Reservations.Add(reservation);

        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/v1/screenings/{screening.Id}/seats");

        // Assert
        response.EnsureSuccessStatusCode();
        
        var seatGrid = await response.Content.ReadFromJsonAsync<SeatGridResponse>();
        
        Assert.NotNull(seatGrid);
        Assert.Equal(seatLayout.Id, seatGrid.SeatLayoutId);
        Assert.Equal(10, seatGrid.Seats.Count());

        // Check sold seat
        var soldSeat = seatGrid.Seats.First(s => s.Label == "A1");
        Assert.Equal("sold", soldSeat.State);

        // Check reserved seat
        var reservedSeat = seatGrid.Seats.First(s => s.Label == "A2");
        Assert.Equal("reserved", reservedSeat.State);

        // Check available seats (all others)
        var availableSeats = seatGrid.Seats.Where(s => s.State == "available").ToList();
        Assert.Equal(8, availableSeats.Count);
    }

    [Fact]
    public async Task GetSeatStatuses_ShouldReturnNotFound_WhenScreeningDoesNotExist()
    {
        // Arrange
        var nonExistentScreeningId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/v1/screenings/{nonExistentScreeningId}/seats");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetSeatStatuses_ShouldReturnNotFound_WhenScreeningIdIsInvalidGuid()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/screenings/invalid-guid/seats");

        // Assert - Invalid GUID returns 404 Not Found due to route constraint
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetSeatStatuses_ShouldReturnCorrectOrder_WhenSeatsAreOrderedByRowAndColumn()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SinemaDbContext>();

        // Clear existing data
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();

        // Create test data
        var cinema = new Cinema
        {
            Id = Guid.NewGuid(),
            Name = "Test Cinema",
            CreatedAt = DateTime.UtcNow
        };

        var hall = new Hall
        {
            Id = Guid.NewGuid(),
            CinemaId = cinema.Id,
            Name = "Test Hall",
            CreatedAt = DateTime.UtcNow
        };

        var seatLayout = new SeatLayout
        {
            Id = Guid.NewGuid(),
            HallId = hall.Id,
            Version = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Create 2x3 seat layout (deliberately out of order to test sorting)
        var seats = new[]
        {
            new Seat { Id = Guid.NewGuid(), SeatLayoutId = seatLayout.Id, Row = 2, Col = 3, Label = "B3" },
            new Seat { Id = Guid.NewGuid(), SeatLayoutId = seatLayout.Id, Row = 1, Col = 1, Label = "A1" },
            new Seat { Id = Guid.NewGuid(), SeatLayoutId = seatLayout.Id, Row = 2, Col = 1, Label = "B1" },
            new Seat { Id = Guid.NewGuid(), SeatLayoutId = seatLayout.Id, Row = 1, Col = 3, Label = "A3" },
            new Seat { Id = Guid.NewGuid(), SeatLayoutId = seatLayout.Id, Row = 1, Col = 2, Label = "A2" },
            new Seat { Id = Guid.NewGuid(), SeatLayoutId = seatLayout.Id, Row = 2, Col = 2, Label = "B2" }
        };

        var movie = new Movie
        {
            Id = Guid.NewGuid(),
            Title = "Test Movie",
            DurationMinutes = 120,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var screening = new Screening
        {
            Id = Guid.NewGuid(),
            MovieId = movie.Id,
            HallId = hall.Id,
            SeatLayoutId = seatLayout.Id,
            StartAt = DateTime.UtcNow.AddHours(2),
            DurationMinutes = 120,
            CreatedAt = DateTime.UtcNow
        };

        context.Cinemas.Add(cinema);
        context.Halls.Add(hall);
        context.SeatLayouts.Add(seatLayout);
        context.Seats.AddRange(seats);
        context.Movies.Add(movie);
        context.Screenings.Add(screening);

        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/v1/screenings/{screening.Id}/seats");

        // Assert
        response.EnsureSuccessStatusCode();
        
        var seatGrid = await response.Content.ReadFromJsonAsync<SeatGridResponse>();
        
        Assert.NotNull(seatGrid);
        var seatList = seatGrid.Seats.ToList();
        
        // Verify order: A1, A2, A3, B1, B2, B3
        Assert.Equal("A1", seatList[0].Label);
        Assert.Equal("A2", seatList[1].Label);
        Assert.Equal("A3", seatList[2].Label);
        Assert.Equal("B1", seatList[3].Label);
        Assert.Equal("B2", seatList[4].Label);
        Assert.Equal("B3", seatList[5].Label);
    }
}
