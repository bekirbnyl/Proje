using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sinema.Domain.Entities;

namespace Sinema.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for Payment entity
/// </summary>
public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Amount)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(p => p.Method)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.ExternalReference)
            .HasMaxLength(500);

        builder.Property(p => p.CreatedAt)
            
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            ;

        // Index for payment tracking
        builder.HasIndex(p => p.ExternalReference);

        // Index for payment status reporting
        builder.HasIndex(p => new { p.Status, p.CreatedAt });

        // Relationships
        builder.HasMany(p => p.Tickets)
            .WithOne(t => t.Payment)
            .HasForeignKey(t => t.PaymentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
