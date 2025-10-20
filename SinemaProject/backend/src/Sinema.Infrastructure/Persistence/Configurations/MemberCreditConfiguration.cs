using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sinema.Domain.Entities;

namespace Sinema.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for MemberCredit entity
/// </summary>
public class MemberCreditConfiguration : IEntityTypeConfiguration<MemberCredit>
{
    public void Configure(EntityTypeBuilder<MemberCredit> builder)
    {
        builder.ToTable("MemberCredits");

        builder.HasKey(mc => mc.Id);

        builder.Property(mc => mc.Amount)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(mc => mc.Type)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(mc => mc.Description)
            .HasMaxLength(500);

        builder.Property(mc => mc.Reference)
            .HasMaxLength(100);

        builder.Property(mc => mc.CreatedAt)
            
            .IsRequired();

        // Index for member credit lookups
        builder.HasIndex(mc => mc.MemberId);

        // Index for credit type reporting
        builder.HasIndex(mc => new { mc.Type, mc.CreatedAt });

        // Index for reference tracking
        builder.HasIndex(mc => mc.Reference);

        // Relationships
        builder.HasOne(mc => mc.Member)
            .WithMany(m => m.Credits)
            .HasForeignKey(mc => mc.MemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
