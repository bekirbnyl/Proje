using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sinema.Domain.Entities;

namespace Sinema.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for Member entity
/// </summary>
public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.ToTable("Members");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.FullName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(m => m.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(m => m.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(m => m.VipStatus)
            .IsRequired();

        builder.Property(m => m.VipStartDate);

        builder.Property(m => m.VipEndDate);

        builder.Property(m => m.VipPaymentId);

        builder.Property(m => m.CreatedAt)
            .IsRequired();

        builder.Property(m => m.UpdatedAt);

        builder.Property(m => m.UserId)
            .HasMaxLength(450); // Standard length for ASP.NET Identity keys

        // Unique email constraint
        builder.HasIndex(m => m.Email)
            .IsUnique();

        // Unique UserId constraint (one member per user)
        builder.HasIndex(m => m.UserId)
            .IsUnique()
            .HasFilter("[UserId] IS NOT NULL");

        // Index for VIP members
        builder.HasIndex(m => m.VipStatus);

        // Index for VIP expiration checking
        builder.HasIndex(m => new { m.VipStatus, m.VipEndDate });

        // Relationships
        builder.HasMany(m => m.Reservations)
            .WithOne(r => r.Member)
            .HasForeignKey(r => r.MemberId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(m => m.Approvals)
            .WithOne(a => a.Member)
            .HasForeignKey(a => a.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.Credits)
            .WithOne(c => c.Member)
            .HasForeignKey(c => c.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        // Foreign key to AspNetUsers (Identity)
        builder.HasOne<Sinema.Infrastructure.Identity.ApplicationUser>()
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
