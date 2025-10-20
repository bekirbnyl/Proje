using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sinema.Domain.Entities;

namespace Sinema.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for SeatHold entity
/// </summary>
public class SeatHoldConfiguration : IEntityTypeConfiguration<SeatHold>
{
    public void Configure(EntityTypeBuilder<SeatHold> builder)
    {
        builder.ToTable("SeatHolds");

        // Primary key
        builder.HasKey(h => h.Id);

        // Properties
        builder.Property(h => h.Id)
            .ValueGeneratedNever();

        builder.Property(h => h.ScreeningId)
            .IsRequired();

        builder.Property(h => h.SeatId)
            .IsRequired();

        builder.Property(h => h.ClientToken)
            .IsRequired()
            .HasMaxLength(64)
            .HasColumnType("nvarchar(64)");

        builder.Property(h => h.UserId)
            .IsRequired(false);

        builder.Property(h => h.CreatedAt)
            .IsRequired()
            ;

        builder.Property(h => h.LastHeartbeatAt)
            .IsRequired()
            ;

        builder.Property(h => h.ExpiresAt)
            .IsRequired()
            ;

        // Constraints
        // UNIQUE constraint on (ScreeningId, SeatId) to ensure only one active hold per seat per screening
        builder.HasIndex(h => new { h.ScreeningId, h.SeatId })
            .IsUnique()
            .HasDatabaseName("IX_SeatHolds_ScreeningId_SeatId");

        // Index for cleanup job performance
        builder.HasIndex(h => h.ExpiresAt)
            .HasDatabaseName("IX_SeatHolds_ExpiresAt");

        // Index for client token lookups
        builder.HasIndex(h => h.ClientToken)
            .HasDatabaseName("IX_SeatHolds_ClientToken");

        // Index for user lookups
        builder.HasIndex(h => h.UserId)
            .HasDatabaseName("IX_SeatHolds_UserId")
            .HasFilter("[UserId] IS NOT NULL");

        // Relationships
        builder.HasOne(h => h.Screening)
            .WithMany()
            .HasForeignKey(h => h.ScreeningId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(h => h.Seat)
            .WithMany()
            .HasForeignKey(h => h.SeatId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent deleting seats that have holds
    }
}
