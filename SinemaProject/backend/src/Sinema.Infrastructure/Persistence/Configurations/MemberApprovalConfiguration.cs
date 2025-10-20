using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sinema.Domain.Entities;

namespace Sinema.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for MemberApproval entity
/// </summary>
public class MemberApprovalConfiguration : IEntityTypeConfiguration<MemberApproval>
{
    public void Configure(EntityTypeBuilder<MemberApproval> builder)
    {
        builder.ToTable("MemberApprovals");

        builder.HasKey(ma => ma.Id);

        builder.Property(ma => ma.Approved)
            .IsRequired();

        builder.Property(ma => ma.Reason)
            .IsRequired()
            .HasMaxLength(400);

        builder.Property(ma => ma.CreatedAt)
            
            .IsRequired();

        // Index for member approval lookups
        builder.HasIndex(ma => ma.MemberId);

        // Index for approval reporting
        builder.HasIndex(ma => new { ma.Approved, ma.CreatedAt });

        // Index for approver tracking
        builder.HasIndex(ma => ma.ApprovedBy);

        // Relationships
        builder.HasOne(ma => ma.Member)
            .WithMany(m => m.Approvals)
            .HasForeignKey(ma => ma.MemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
