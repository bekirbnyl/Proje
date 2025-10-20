using Microsoft.EntityFrameworkCore;
using Sinema.Domain.Entities;
using Sinema.Domain.Enums;
using Sinema.Infrastructure.Persistence;
using Sinema.Infrastructure.Services.Seating;
using Xunit;

namespace Sinema.UnitTests.Seating;

/// <summary>
/// Unit tests for SeatStatusService
/// </summary>
public class SeatStatusService_Tests : IDisposable
{
    private readonly SinemaDbContext _context;
    private readonly SeatStatusService _seatStatusService;

    public SeatStatusService_Tests()
    {
        var options = new DbContextOptionsBuilder<SinemaDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new SinemaDbContext(options);
        _seatStatusService = new SeatStatusService(_context);
    }

    [Fact]
    public async Task GetSeatStatusesAsync_ShouldReturnCorrectStates_WhenSeatsHaveDifferentStatuses()
    {
        // Arrange
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

        var seats = new[]
        {
            new Seat { Id = Guid.NewGuid(), SeatLayoutId = seatLayout.Id, Row = 1, Col = 1, Label = "A1" },
            new Seat { Id = Guid.NewGuid(), SeatLayoutId = seatLayout.Id, Row = 1, Col = 2, Label = "A2" },
            new Seat { Id = Guid.NewGuid(), SeatLayoutId = seatLayout.Id, Row = 1, Col = 3, Label = "A3" },
            new Seat { Id = Guid.NewGuid(), SeatLayoutId = seatLayout.Id, Row = 1, Col = 4, Label = "A4" },
            new Seat { Id = Guid.NewGuid(), SeatLayoutId = seatLayout.Id, Row = 1, Col = 5, Label = "A5" }
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

        // A1 - sold ticket
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            ScreeningId = screening.Id,
            SeatId = seats[0].Id,
            Type = TicketType.Full,
            Channel = TicketChannel.Web,
            Price = 25.00m,
            SoldAt = DateTime.UtcNow
        };

        // A2 - active reservation
        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            ScreeningId = screening.Id,
            SeatId = seats[1].Id,
            Status = ReservationStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30),
            CreatedAt = DateTime.UtcNow
        };

        // A3, A4, A5 - available (no tickets or reservations)

        _context.Cinemas.Add(cinema);
        _context.Halls.Add(hall);
        _context.SeatLayouts.Add(seatLayout);
        _context.Seats.AddRange(seats);
        _context.Movies.Add(movie);
        _context.Screenings.Add(screening);
        _context.Tickets.Add(ticket);
        _context.Reservations.Add(reservation);

        await _context.SaveChangesAsync();

        // Act
        var result = await _seatStatusService.GetSeatStatusesAsync(screening.Id);
        var seatStatuses = result.ToList();

        // Assert
        Assert.Equal(5, seatStatuses.Count);

        // A1 should be sold
        var seatA1 = seatStatuses.First(s => s.Label == "A1");
        Assert.Equal("sold", seatA1.State);
        Assert.Equal(seats[0].Id, seatA1.SeatId);

        // A2 should be reserved
        var seatA2 = seatStatuses.First(s => s.Label == "A2");
        Assert.Equal("reserved", seatA2.State);
        Assert.Equal(seats[1].Id, seatA2.SeatId);

        // A3, A4, A5 should be available
        var availableSeats = seatStatuses.Where(s => s.State == "available").ToList();
        Assert.Equal(3, availableSeats.Count);
        Assert.Contains(availableSeats, s => s.Label == "A3");
        Assert.Contains(availableSeats, s => s.Label == "A4");
        Assert.Contains(availableSeats, s => s.Label == "A5");
    }

    [Fact]
    public async Task GetSeatStatusesAsync_ShouldIgnoreExpiredReservations()
    {
        // Arrange
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

        var seats = new[]
        {
            new Seat { Id = Guid.NewGuid(), SeatLayoutId = seatLayout.Id, Row = 1, Col = 1, Label = "A1" },
            new Seat { Id = Guid.NewGuid(), SeatLayoutId = seatLayout.Id, Row = 1, Col = 2, Label = "A2" }
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

        // A1 - expired reservation (should be treated as available)
        var expiredReservation = new Reservation
        {
            Id = Guid.NewGuid(),
            ScreeningId = screening.Id,
            SeatId = seats[0].Id,
            Status = ReservationStatus.Expired,
            ExpiresAt = DateTime.UtcNow.AddMinutes(-10),
            CreatedAt = DateTime.UtcNow.AddMinutes(-40)
        };

        // A2 - canceled reservation (should be treated as available)
        var canceledReservation = new Reservation
        {
            Id = Guid.NewGuid(),
            ScreeningId = screening.Id,
            SeatId = seats[1].Id,
            Status = ReservationStatus.Canceled,
            ExpiresAt = DateTime.UtcNow.AddMinutes(20),
            CreatedAt = DateTime.UtcNow.AddMinutes(-10)
        };

        _context.Cinemas.Add(cinema);
        _context.Halls.Add(hall);
        _context.SeatLayouts.Add(seatLayout);
        _context.Seats.AddRange(seats);
        _context.Movies.Add(movie);
        _context.Screenings.Add(screening);
        _context.Reservations.AddRange(expiredReservation, canceledReservation);

        await _context.SaveChangesAsync();

        // Act
        var result = await _seatStatusService.GetSeatStatusesAsync(screening.Id);
        var seatStatuses = result.ToList();

        // Assert
        Assert.Equal(2, seatStatuses.Count);
        Assert.All(seatStatuses, seat => Assert.Equal("available", seat.State));
    }

    [Fact]
    public async Task GetSeatStatusesAsync_ShouldThrowException_WhenScreeningNotFound()
    {
        // Arrange
        var nonExistentScreeningId = Guid.NewGuid();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _seatStatusService.GetSeatStatusesAsync(nonExistentScreeningId));

        Assert.Contains("Screening with ID", exception.Message);
        Assert.Contains("not found", exception.Message);
    }

    [Fact]
    public async Task GetSeatGridAsync_ShouldReturnCorrectSeatLayoutId()
    {
        // Arrange
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

        var seat = new Seat
        {
            Id = Guid.NewGuid(),
            SeatLayoutId = seatLayout.Id,
            Row = 1,
            Col = 1,
            Label = "A1"
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

        _context.Cinemas.Add(cinema);
        _context.Halls.Add(hall);
        _context.SeatLayouts.Add(seatLayout);
        _context.Seats.Add(seat);
        _context.Movies.Add(movie);
        _context.Screenings.Add(screening);

        await _context.SaveChangesAsync();

        // Act
        var result = await _seatStatusService.GetSeatGridAsync(screening.Id);

        // Assert
        Assert.Equal(seatLayout.Id, result.SeatLayoutId);
        Assert.Single(result.Seats);
        Assert.Equal("A1", result.Seats.First().Label);
        Assert.Equal("available", result.Seats.First().State);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
