using Microsoft.EntityFrameworkCore;
using Sinema.Application.DTOs.Reports.Sales;
using Sinema.Domain.Entities;
using Sinema.Domain.Enums;
using Sinema.Infrastructure.Persistence;
using Sinema.Infrastructure.Services.Reporting;

namespace Sinema.UnitTests.Reports;

/// <summary>
/// Unit tests for sales report aggregation logic
/// </summary>
public class SalesReport_Aggregation_Tests : IDisposable
{
    private readonly SinemaDbContext _context;
    private readonly SalesReportBuilder _builder;

    public SalesReport_Aggregation_Tests()
    {
        var options = new DbContextOptionsBuilder<SinemaDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new SinemaDbContext(options);
        _builder = new SalesReportBuilder(_context);
    }

    [Fact]
    public async Task BuildReportAsync_WithSalesData_CalculatesCorrectMetrics()
    {
        // Arrange
        await SeedTestData();

        var request = new SalesReportRequest
        {
            From = DateTime.Today.AddDays(-7),
            To = DateTime.Today,
            Grain = "daily",
            By = "film",
            Channel = "all"
        };

        // Act
        var result = await _builder.BuildReportAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Data.Count > 0);
        Assert.True(result.Summary.TotalTickets > 0);
        Assert.True(result.Summary.TotalNet > 0);
    }

    [Fact]
    public async Task BuildReportAsync_GroupByFilm_GroupsCorrectly()
    {
        // Arrange
        await SeedTestData();

        var request = new SalesReportRequest
        {
            From = DateTime.Today.AddDays(-7),
            To = DateTime.Today,
            Grain = "daily",
            By = "film",
            Channel = "all"
        };

        // Act
        var result = await _builder.BuildReportAsync(request);

        // Assert
        Assert.All(result.Data, row => Assert.NotNull(row.FilmName));
    }

    private async Task SeedTestData()
    {
        var cinema = new Cinema { Id = Guid.NewGuid(), Name = "Test Cinema", CreatedAt = DateTime.UtcNow };
        var hall = new Hall { Id = Guid.NewGuid(), Name = "Hall 1", CinemaId = cinema.Id, CreatedAt = DateTime.UtcNow };
        var movie = new Movie { Id = Guid.NewGuid(), Title = "Test Movie", DurationMinutes = 120, IsActive = true, CreatedAt = DateTime.UtcNow };
        
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
            Row = "A", 
            Number = 1, 
            CreatedAt = DateTime.UtcNow 
        };

        var screening = new Screening
        {
            Id = Guid.NewGuid(),
            MovieId = movie.Id,
            HallId = hall.Id,
            SeatLayoutId = seatLayout.Id,
            StartAt = DateTime.Today.AddDays(-1),
            DurationMinutes = 120,
            CreatedAt = DateTime.UtcNow
        };

        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            ScreeningId = screening.Id,
            SeatId = seat.Id,
            Type = TicketType.Full,
            Channel = TicketChannel.Online,
            Price = 50.00m,
            SoldAt = DateTime.Today.AddDays(-1),
            TicketCode = "TEST001"
        };

        _context.Cinemas.Add(cinema);
        _context.Halls.Add(hall);
        _context.Movies.Add(movie);
        _context.SeatLayouts.Add(seatLayout);
        _context.Seats.Add(seat);
        _context.Screenings.Add(screening);
        _context.Tickets.Add(ticket);

        await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
