using Microsoft.EntityFrameworkCore;
using Sinema.Domain.Entities;
using Sinema.Domain.Interfaces.Services;

namespace Sinema.Infrastructure.Persistence.Seed;

/// <summary>
/// Service responsible for seeding initial data into the database
/// </summary>
public class DataSeeder
{
    private readonly IClock _clock;

    public DataSeeder(IClock clock)
    {
        _clock = clock;
    }

    /// <summary>
    /// Seeds the database with initial data
    /// </summary>
    /// <param name="context">Database context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task SeedAsync(SinemaDbContext context, CancellationToken cancellationToken = default)
    {
        await SeedSettingsAsync(context, cancellationToken);
        await SeedCinemasAndHallsAsync(context, cancellationToken);
        await SeedMoviesAsync(context, cancellationToken);
        await SeedMembersAsync(context, cancellationToken);
        await SeedScreeningsAsync(context, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedSettingsAsync(SinemaDbContext context, CancellationToken cancellationToken)
    {
        if (await context.Settings.AnyAsync(s => s.Key == Setting.Keys.HalkGunu, cancellationToken))
        {
            return;
        }

        var settings = new[]
        {
            Setting.Create(Setting.Keys.HalkGunu, "Wednesday", "Day of the week for Halk Günü discount"),
            Setting.Create(Setting.Keys.ReservationTimeoutMinutes, "30", "Minutes before screening that reservations expire"),
            Setting.Create(Setting.Keys.ReservationCutoffMinutes, "60", "Minutes before screening that reservations are disabled"),
            Setting.Create(Setting.Keys.VipAdvanceBookingDays, "7", "Days in advance VIP members can book"),
            Setting.Create(Setting.Keys.RegularAdvanceBookingDays, "2", "Days in advance regular users can book"),
            Setting.Create(Setting.Keys.MinCreditTopUpAmount, "250.00", "Minimum amount for credit top-up (5 film equivalent)")
        };

        context.Settings.AddRange(settings);
    }

    private async Task SeedCinemasAndHallsAsync(SinemaDbContext context, CancellationToken cancellationToken)
    {
        if (await context.Cinemas.AnyAsync(c => c.Name == "Merit Sinema", cancellationToken))
        {
            return;
        }

        var cinema = new Cinema
        {
            Id = Guid.NewGuid(),
            Name = "Merit Sinema",
            CreatedAt = _clock.UtcNow
        };

        context.Cinemas.Add(cinema);
        await context.SaveChangesAsync(cancellationToken); // Save cinema first to get ID

        var halls = new[]
        {
            new Hall
            {
                Id = Guid.NewGuid(),
                CinemaId = cinema.Id,
                Name = "Salon 1",
                CreatedAt = _clock.UtcNow
            },
            new Hall
            {
                Id = Guid.NewGuid(),
                CinemaId = cinema.Id,
                Name = "Salon 2",
                CreatedAt = _clock.UtcNow
            }
        };

        context.Halls.AddRange(halls);
        await context.SaveChangesAsync(cancellationToken); // Save halls to get IDs

        // Create seat layout for Salon 1 (5x10 seats)
        var salon1 = halls[0];
        var seatLayout1 = new SeatLayout
        {
            Id = Guid.NewGuid(),
            HallId = salon1.Id,
            Version = 1,
            IsActive = true,
            CreatedAt = _clock.UtcNow
        };

        // Create seat layout for Salon 2 (6x12 seats)
        var salon2 = halls[1];
        var seatLayout2 = new SeatLayout
        {
            Id = Guid.NewGuid(),
            HallId = salon2.Id,
            Version = 1,
            IsActive = true,
            CreatedAt = _clock.UtcNow
        };

        context.SeatLayouts.AddRange(new[] { seatLayout1, seatLayout2 });
        await context.SaveChangesAsync(cancellationToken); // Save layouts to get IDs

        // Create 5x10 seats for Salon 1 (A1-A10, B1-B10, C1-C10, D1-D10, E1-E10)
        var seats = new List<Seat>();
        for (int row = 1; row <= 5; row++)
        {
            for (int col = 1; col <= 10; col++)
            {
                var seat = new Seat
                {
                    Id = Guid.NewGuid(),
                    SeatLayoutId = seatLayout1.Id,
                    Row = row,
                    Col = col
                };
                seat.GenerateLabel(); // This will create A1, A2, B1, etc.
                seats.Add(seat);
            }
        }

        // Create 6x12 seats for Salon 2 (A1-A12, B1-B12, C1-C12, D1-D12, E1-E12, F1-F12)
        for (int row = 1; row <= 6; row++)
        {
            for (int col = 1; col <= 12; col++)
            {
                var seat = new Seat
                {
                    Id = Guid.NewGuid(),
                    SeatLayoutId = seatLayout2.Id,
                    Row = row,
                    Col = col
                };
                seat.GenerateLabel(); // This will create A1, A2, B1, etc.
                seats.Add(seat);
            }
        }

        context.Seats.AddRange(seats);
    }

    private async Task SeedMoviesAsync(SinemaDbContext context, CancellationToken cancellationToken)
    {
        if (await context.Movies.AnyAsync(m => m.Title == "Örnek Film", cancellationToken))
        {
            return;
        }

        var movie = new Movie
        {
            Id = Guid.NewGuid(),
            Title = "Örnek Film",
            DurationMinutes = 120,
            IsActive = true,
            CreatedAt = _clock.UtcNow
        };

        context.Movies.Add(movie);
    }

    private async Task SeedMembersAsync(SinemaDbContext context, CancellationToken cancellationToken)
    {
        if (await context.Members.AnyAsync(m => m.Email == "test@example.com", cancellationToken))
        {
            return;
        }

        var member = new Member
        {
            Id = Guid.NewGuid(),
            FullName = "Test Üye",
            Email = "test@example.com",
            PhoneNumber = "+90 555 123 4567",
            VipStatus = false,
            CreatedAt = _clock.UtcNow
        };

        context.Members.Add(member);
        await context.SaveChangesAsync(cancellationToken); // Save member to get ID

        var approval = MemberApproval.CreateApproval(
            member.Id,
            "Seed approval - Initial test member",
            null
        );

        context.MemberApprovals.Add(approval);
    }

    private async Task SeedScreeningsAsync(SinemaDbContext context, CancellationToken cancellationToken)
    {
        // Check if we already have screenings
        if (await context.Screenings.AnyAsync(cancellationToken))
        {
            return;
        }

        // Get the required entities
        var cinema = await context.Cinemas
            .Include(c => c.Halls)
            .ThenInclude(h => h.SeatLayouts)
            .FirstOrDefaultAsync(c => c.Name == "Merit Sinema", cancellationToken);

        var movie = await context.Movies
            .FirstOrDefaultAsync(m => m.Title == "Örnek Film", cancellationToken);

        if (cinema == null || movie == null)
        {
            return;
        }

        var salon1 = cinema.Halls.FirstOrDefault(h => h.Name == "Salon 1");
        var activeSeatLayout = salon1?.SeatLayouts.FirstOrDefault(sl => sl.IsActive);

        if (salon1 == null || activeSeatLayout == null)
        {
            // Try Salon 2 if Salon 1 doesn't have layout
            var salon2 = cinema.Halls.FirstOrDefault(h => h.Name == "Salon 2");
            var activeSeatLayout2 = salon2?.SeatLayouts.FirstOrDefault(sl => sl.IsActive);
            
            if (salon2 == null || activeSeatLayout2 == null)
            {
                return;
            }
            
            salon1 = salon2;
            activeSeatLayout = activeSeatLayout2;
        }

        // Create a screening for tomorrow evening at 19:00 UTC
        var tomorrowEvening = _clock.UtcNow.Date.AddDays(1).AddHours(19);

        var screening = new Screening
        {
            Id = Guid.NewGuid(),
            MovieId = movie.Id,
            HallId = salon1.Id,
            SeatLayoutId = activeSeatLayout.Id,
            StartAt = tomorrowEvening,
            DurationMinutes = movie.DurationMinutes,
            CreatedAt = _clock.UtcNow
        };

        context.Screenings.Add(screening);
    }
}
