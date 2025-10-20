using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sinema.Domain.Entities;

namespace Sinema.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for Reservation entity
/// </summary>
public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("Reservations");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(r => r.ExpiresAt)
            
            .IsRequired();

        builder.Property(r => r.CreatedAt)
            
            .IsRequired();

        // Row version for optimistic concurrency
        builder.Property(r => r.RowVersion)
            .IsRowVersion();

        // UNIQUE constraint for screening-seat combination
        builder.HasIndex(r => new { r.ScreeningId, r.SeatId })
            .IsUnique()
            .HasDatabaseName("IX_Reservations_Screening_Seat_Unique");

        // Index for member reservations
        builder.HasIndex(r => r.MemberId);

        // Index for expired reservations cleanup
        builder.HasIndex(r => new { r.Status, r.ExpiresAt });

        // Relationships
        builder.HasOne(r => r.Screening)
            .WithMany(s => s.Reservations)
            .HasForeignKey(r => r.ScreeningId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Seat)
            .WithMany(s => s.Reservations)
            .HasForeignKey(r => r.SeatId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Member)
            .WithMany(m => m.Reservations)
            .HasForeignKey(r => r.MemberId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
    }
}
