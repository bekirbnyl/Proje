using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sinema.Domain.Entities;
using Sinema.Infrastructure.Identity;
using Sinema.Infrastructure.Persistence.Entities;
using Sinema.Infrastructure.Persistence.QueryFilters;

namespace Sinema.Infrastructure.Persistence;

/// <summary>
/// Main database context for the Sinema application with Identity
/// </summary>
public class SinemaDbContext : IdentityDbContext<ApplicationUser>
{
    public SinemaDbContext(DbContextOptions<SinemaDbContext> options) : base(options)
    {
    }

    // Cinema-related entities
    public DbSet<Cinema> Cinemas => Set<Cinema>();
    public DbSet<Hall> Halls => Set<Hall>();
    public DbSet<SeatLayout> SeatLayouts => Set<SeatLayout>();
    public DbSet<Seat> Seats => Set<Seat>();

    // Movie and screening entities
    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<Screening> Screenings => Set<Screening>();

    // Reservation and ticket entities
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<SeatHold> SeatHolds => Set<SeatHold>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<Payment> Payments => Set<Payment>();

    // Member-related entities
    public DbSet<Member> Members => Set<Member>();
    public DbSet<MemberApproval> MemberApprovals => Set<MemberApproval>();
    public DbSet<MemberCredit> MemberCredits => Set<MemberCredit>();

    // System entities
    public DbSet<Setting> Settings => Set<Setting>();
    public DbSet<SettingsChangeLog> SettingsChangeLogs => Set<SettingsChangeLog>();
    public DbSet<DeletionLog> DeletionLogs => Set<DeletionLog>();

    // Report entities
    public DbSet<ReportJob> ReportJobs => Set<ReportJob>();
    public DbSet<ReportLog> ReportLogs => Set<ReportLog>();

    // Auth-related entities
    public DbSet<UserRefreshToken> UserRefreshTokens => Set<UserRefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply all entity configurations from this assembly
        builder.ApplyConfigurationsFromAssembly(typeof(SinemaDbContext).Assembly);

        // Apply soft delete query filters
        builder.ApplySoftDeleteFilters();

        // Configure Identity table names with AspNet prefix (standard)
        builder.Entity<ApplicationUser>().ToTable("AspNetUsers");
        builder.Entity<IdentityRole>().ToTable("AspNetRoles");
        builder.Entity<IdentityUserRole<string>>().ToTable("AspNetUserRoles");
        builder.Entity<IdentityUserClaim<string>>().ToTable("AspNetUserClaims");
        builder.Entity<IdentityUserLogin<string>>().ToTable("AspNetUserLogins");
        builder.Entity<IdentityUserToken<string>>().ToTable("AspNetUserTokens");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("AspNetRoleClaims");
    }
}