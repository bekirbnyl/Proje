using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sinema.Domain.Entities;

namespace Sinema.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for Ticket entity
/// </summary>
public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("Tickets");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(t => t.Channel)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(t => t.Price)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(t => t.SoldAt)
            .IsRequired();

        builder.Property(t => t.TicketCode)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(t => t.AppliedPricingJson)
            .IsRequired(false);

        // Row version for optimistic concurrency
        builder.Property(t => t.RowVersion)
            .IsRowVersion();

        // UNIQUE constraint for screening-seat combination
        builder.HasIndex(t => new { t.ScreeningId, t.SeatId })
            .IsUnique()
            .HasDatabaseName("IX_Tickets_Screening_Seat_Unique");

        // UNIQUE constraint for ticket code
        builder.HasIndex(t => t.TicketCode)
            .IsUnique()
            .HasDatabaseName("IX_Tickets_TicketCode_Unique");

        // Index for sales reporting
        builder.HasIndex(t => new { t.Channel, t.SoldAt });

        // Index for payment tracking
        builder.HasIndex(t => t.PaymentId);

        // Relationships
        builder.HasOne(t => t.Screening)
            .WithMany(s => s.Tickets)
            .HasForeignKey(t => t.ScreeningId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Seat)
            .WithMany(s => s.Tickets)
            .HasForeignKey(t => t.SeatId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Payment)
            .WithMany(p => p.Tickets)
            .HasForeignKey(t => t.PaymentId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
    }
}
