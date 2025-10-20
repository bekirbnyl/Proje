using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sinema.Application.DTOs.Reservations.Holds;
using Sinema.Domain.Entities;
using Sinema.Domain.Enums;
using Sinema.Infrastructure.Persistence;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Sinema.IntegrationTests.Api;

/// <summary>
/// Integration tests for seat holds and reservations concurrency scenarios
/// </summary>
public class Holds_And_Reservations_Concurrency_Tests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public Holds_And_Reservations_Concurrency_Tests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task CreateHolds_TwoClientsForSameSeat_ShouldReturnConflictForSecond()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SinemaDbContext>();
        
        var screening = await CreateTestScreeningAsync(context);
        var seats = await context.Seats
            .Where(s => s.SeatLayoutId == screening.SeatLayoutId)
            .Take(1)
            .ToListAsync();

        var clientToken1 = "client-1-" + Guid.NewGuid();
        var clientToken2 = "client-2-" + Guid.NewGuid();

        var holdRequest1 = new CreateSeatHoldsRequest
        {
            ScreeningId = screening.Id,
            SeatIds = new List<Guid> { seats[0].Id },
            ClientToken = clientToken1
        };

        var holdRequest2 = new CreateSeatHoldsRequest
        {
            ScreeningId = screening.Id,
            SeatIds = new List<Guid> { seats[0].Id },
            ClientToken = clientToken2
        };

        // Act - Make concurrent requests
        var task1 = _client.PostAsJsonAsync("/api/v1/holds", holdRequest1);
        var task2 = _client.PostAsJsonAsync("/api/v1/holds", holdRequest2);

        var responses = await Task.WhenAll(task1, task2);

        // Assert
        var successfulResponses = responses.Where(r => r.IsSuccessStatusCode).ToArray();
        var conflictResponses = responses.Where(r => r.StatusCode == HttpStatusCode.Conflict).ToArray();

        successfulResponses.Should().HaveCount(1, "Only one client should successfully hold the seat");
        conflictResponses.Should().HaveCount(1, "One client should receive a conflict response");

        // Verify the successful response contains hold information
        var successResponse = successfulResponses[0];
        var holdResult = await successResponse.Content.ReadFromJsonAsync<List<SeatHoldDto>>();
        holdResult.Should().NotBeNull();
        holdResult.Should().HaveCount(1);
        holdResult![0].SeatId.Should().Be(seats[0].Id);
    }

    [Fact]
    public async Task CreateReservation_WithoutValidHold_ShouldReturnConflict()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SinemaDbContext>();
        
        var screening = await CreateTestScreeningAsync(context);
        var seats = await context.Seats
            .Where(s => s.SeatLayoutId == screening.SeatLayoutId)
            .Take(1)
            .ToListAsync();

        var reservationRequest = new
        {
            ScreeningId = screening.Id,
            SeatIds = new List<Guid> { seats[0].Id },
            ClientToken = "client-without-hold-" + Guid.NewGuid()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/reservations", reservationRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("must be held");
    }

    [Fact]
    public async Task FullFlow_HoldThenReserve_ShouldSucceed()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SinemaDbContext>();
        
        var screening = await CreateTestScreeningAsync(context);
        var seats = await context.Seats
            .Where(s => s.SeatLayoutId == screening.SeatLayoutId)
            .Take(2)
            .ToListAsync();

        var clientToken = "client-full-flow-" + Guid.NewGuid();

        // Act 1: Create holds
        var holdRequest = new CreateSeatHoldsRequest
        {
            ScreeningId = screening.Id,
            SeatIds = seats.Select(s => s.Id).ToList(),
            ClientToken = clientToken
        };

        var holdResponse = await _client.PostAsJsonAsync("/api/v1/holds", holdRequest);

        // Assert 1: Holds created successfully
        holdResponse.Should().BeSuccessful();
        var holds = await holdResponse.Content.ReadFromJsonAsync<List<SeatHoldDto>>();
        holds.Should().NotBeNull();
        holds.Should().HaveCount(2);

        // Act 2: Create reservation
        var reservationRequest = new
        {
            ScreeningId = screening.Id,
            SeatIds = seats.Select(s => s.Id).ToList(),
            ClientToken = clientToken
        };

        var reservationResponse = await _client.PostAsJsonAsync("/api/v1/reservations", reservationRequest);

        // Assert 2: Reservation created successfully
        reservationResponse.Should().BeSuccessful();
        
        // Verify holds are removed after reservation creation
        var dbHolds = await context.SeatHolds
            .Where(h => h.ClientToken == clientToken)
            .ToListAsync();
        dbHolds.Should().BeEmpty("Holds should be removed after reservation creation");

        // Verify reservations exist
        var dbReservations = await context.Reservations
            .Where(r => r.ScreeningId == screening.Id && seats.Select(s => s.Id).Contains(r.SeatId))
            .ToListAsync();
        dbReservations.Should().HaveCount(2);
        dbReservations.All(r => r.Status == ReservationStatus.Pending).Should().BeTrue();
    }

    [Fact]
    public async Task ExtendHold_WithValidClientToken_ShouldSucceed()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SinemaDbContext>();
        
        var screening = await CreateTestScreeningAsync(context);
        var seats = await context.Seats
            .Where(s => s.SeatLayoutId == screening.SeatLayoutId)
            .Take(1)
            .ToListAsync();

        var clientToken = "client-extend-" + Guid.NewGuid();

        // Create hold first
        var holdRequest = new CreateSeatHoldsRequest
        {
            ScreeningId = screening.Id,
            SeatIds = new List<Guid> { seats[0].Id },
            ClientToken = clientToken
        };

        var holdResponse = await _client.PostAsJsonAsync("/api/v1/holds", holdRequest);
        holdResponse.Should().BeSuccessful();

        var holds = await holdResponse.Content.ReadFromJsonAsync<List<SeatHoldDto>>();
        var holdId = holds![0].HoldId;
        var originalExpiry = holds[0].ExpiresAt;

        // Act: Extend hold
        var extendRequest = new { ClientToken = clientToken };
        var extendResponse = await _client.PostAsJsonAsync($"/api/v1/holds/{holdId}/extend", extendRequest);

        // Assert
        extendResponse.Should().BeSuccessful();
        var extendedHold = await extendResponse.Content.ReadFromJsonAsync<SeatHoldDto>();
        extendedHold.Should().NotBeNull();
        extendedHold!.ExpiresAt.Should().BeAfter(originalExpiry);
    }

    private async Task<Screening> CreateTestScreeningAsync(SinemaDbContext context)
    {
        // Create or get test data
        var cinema = new Cinema { Id = Guid.NewGuid(), Name = "Test Cinema" };
        var hall = new Hall { Id = Guid.NewGuid(), Name = "Test Hall", CinemaId = cinema.Id };
        var seatLayout = new SeatLayout
        {
            Id = Guid.NewGuid(),
            Name = "Test Layout",
            HallId = hall.Id,
            Rows = 3,
            Columns = 3,
            IsActive = true
        };

        // Create some seats
        var seats = new List<Seat>();
        for (int row = 1; row <= 3; row++)
        {
            for (int col = 1; col <= 3; col++)
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
            StartAt = DateTime.UtcNow.AddHours(3), // Future screening
            DurationMinutes = movie.DurationMinutes,
            CreatedAt = DateTime.UtcNow
        };

        context.Cinemas.Add(cinema);
        context.Halls.Add(hall);
        context.SeatLayouts.Add(seatLayout);
        context.Seats.AddRange(seats);
        context.Movies.Add(movie);
        context.Screenings.Add(screening);

        await context.SaveChangesAsync();

        return screening;
    }
}
