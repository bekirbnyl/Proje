using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Identity;
using Sinema.Infrastructure.Persistence;
using Sinema.Infrastructure.Identity;
using Sinema.Domain.Entities;

// Console app to fix missing database data
var builder = Host.CreateApplicationBuilder(args);

// Add services - using MSSQL
var connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=SinemaDb_Dev;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true";
builder.Services.AddDbContext<SinemaDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<SinemaDbContext>()
    .AddDefaultTokenProviders();

var app = builder.Build();

using var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<SinemaDbContext>();

Console.WriteLine("=== Fixing Database Data ===");

// 1. Check if we have Salon 2 layout
var salon2 = await context.Halls.FirstOrDefaultAsync(h => h.Name == "Salon 2");
if (salon2 != null)
{
    var salon2Layout = await context.SeatLayouts.FirstOrDefaultAsync(sl => sl.HallId == salon2.Id);
    if (salon2Layout == null)
    {
        Console.WriteLine("Creating layout for Salon 2...");
        
        var seatLayout2 = new SeatLayout
        {
            Id = Guid.NewGuid(),
            HallId = salon2.Id,
            Version = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.SeatLayouts.Add(seatLayout2);
        await context.SaveChangesAsync();

        // Create 6x12 seats for Salon 2
        var seats = new List<Seat>();
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
                seat.GenerateLabel();
                seats.Add(seat);
            }
        }

        context.Seats.AddRange(seats);
        await context.SaveChangesAsync();
        
        Console.WriteLine($"✅ Created layout for Salon 2 with {seats.Count} seats");
    }
    else
    {
        Console.WriteLine("ℹ️ Salon 2 already has a layout");
    }
}

// 2. Check if we have enough movies for testing
var movieCount = await context.Movies.CountAsync();
if (movieCount < 3)
{
    Console.WriteLine("Adding more test movies...");
    
    var movies = new[]
    {
        new Movie
        {
            Id = Guid.NewGuid(),
            Title = "Avatar 3",
            DurationMinutes = 180,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        },
        new Movie
        {
            Id = Guid.NewGuid(),
            Title = "John Wick 5",
            DurationMinutes = 120,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        },
        new Movie
        {
            Id = Guid.NewGuid(),
            Title = "Spider-Man: No Way Home 2",
            DurationMinutes = 150,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        }
    };

    context.Movies.AddRange(movies);
    await context.SaveChangesAsync();
    
    Console.WriteLine($"✅ Added {movies.Length} new movies");
}

// 3. Check essential settings
var settings = new Dictionary<string, string>
{
    { "VipAdvanceBookingDays", "7" },
    { "RegularAdvanceBookingDays", "2" },
    { "ReservationTimeoutMinutes", "30" },
    { "ReservationCutoffMinutes", "60" },
    { "HalkGunu", "Wednesday" },
    { "MinCreditTopUpAmount", "250.00" },
    { "BaseTicketPrice", "100.00" }
};

foreach (var (key, value) in settings)
{
    var existingSetting = await context.Settings.FirstOrDefaultAsync(s => s.Key == key);
    if (existingSetting == null)
    {
        var setting = Setting.Create(key, value, $"Auto-created setting for {key}");
        context.Settings.Add(setting);
        Console.WriteLine($"✅ Added setting: {key} = {value}");
    }
}

await context.SaveChangesAsync();

// 4. Summary
var halls = await context.Halls.Include(h => h.SeatLayouts).ToListAsync();
var allMovies = await context.Movies.ToListAsync();
var settingsCount = await context.Settings.CountAsync();

Console.WriteLine("\n=== Database Summary ===");
Console.WriteLine($"Halls: {halls.Count}");
foreach (var hall in halls)
{
    var layoutCount = hall.SeatLayouts.Count(sl => sl.IsActive);
    Console.WriteLine($"  - {hall.Name}: {layoutCount} active layout(s)");
}
Console.WriteLine($"Movies: {allMovies.Count}");
Console.WriteLine($"Settings: {settingsCount}");

Console.WriteLine("\n=== Done ===");
