using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sinema.Domain.Entities;

namespace Sinema.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for Screening entity
/// </summary>
public class ScreeningConfiguration : IEntityTypeConfiguration<Screening>
{
    public void Configure(EntityTypeBuilder<Screening> builder)
    {
        builder.ToTable("Screenings");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.StartAt)
            
            .IsRequired();

        builder.Property(s => s.DurationMinutes)
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            
            .IsRequired();

        // Soft delete properties
        builder.Property(s => s.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(s => s.DeletedAt)
            ;

        builder.Property(s => s.DeletedBy);

        // INDEX for hall scheduling conflicts
        builder.HasIndex(s => new { s.HallId, s.StartAt })
            .HasDatabaseName("IX_Screenings_Hall_StartTime");

        // Index for movie screenings
        builder.HasIndex(s => s.MovieId);

        // Index for seat layout screenings
        builder.HasIndex(s => s.SeatLayoutId);

        // Index for soft delete filtering
        builder.HasIndex(s => s.IsDeleted)
            .HasDatabaseName("IX_Screenings_IsDeleted");

        // Index for deleted screenings by date
        builder.HasIndex(s => s.DeletedAt)
            .HasDatabaseName("IX_Screenings_DeletedAt");

        // Relationships
        builder.HasOne(s => s.Movie)
            .WithMany(m => m.Screenings)
            .HasForeignKey(s => s.MovieId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Hall)
            .WithMany(h => h.Screenings)
            .HasForeignKey(s => s.HallId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.SeatLayout)
            .WithMany(sl => sl.Screenings)
            .HasForeignKey(s => s.SeatLayoutId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.Reservations)
            .WithOne(r => r.Screening)
            .HasForeignKey(r => r.ScreeningId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.Tickets)
            .WithOne(t => t.Screening)
            .HasForeignKey(t => t.ScreeningId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
