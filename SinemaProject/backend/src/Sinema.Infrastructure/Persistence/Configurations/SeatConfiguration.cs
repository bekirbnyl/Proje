using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sinema.Domain.Entities;

namespace Sinema.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for Seat entity
/// </summary>
public class SeatConfiguration : IEntityTypeConfiguration<Seat>
{
    public void Configure(EntityTypeBuilder<Seat> builder)
    {
        builder.ToTable("Seats");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Row)
            .IsRequired();

        builder.Property(s => s.Col)
            .IsRequired();

        builder.Property(s => s.Label)
            .IsRequired()
            .HasMaxLength(50);

        // UNIQUE constraint for seat position within layout
        builder.HasIndex(s => new { s.SeatLayoutId, s.Row, s.Col })
            .IsUnique();

        // Index for fast seat lookups
        builder.HasIndex(s => new { s.SeatLayoutId, s.Label });

        // Relationships
        builder.HasOne(s => s.SeatLayout)
            .WithMany(sl => sl.Seats)
            .HasForeignKey(s => s.SeatLayoutId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Reservations)
            .WithOne(r => r.Seat)
            .HasForeignKey(r => r.SeatId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(s => s.Tickets)
            .WithOne(t => t.Seat)
            .HasForeignKey(t => t.SeatId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
