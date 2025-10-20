using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sinema.Domain.Entities;

namespace Sinema.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for DeletionLog entity
/// </summary>
public class DeletionLogConfiguration : IEntityTypeConfiguration<DeletionLog>
{
    public void Configure(EntityTypeBuilder<DeletionLog> builder)
    {
        builder.ToTable("DeletionLogs");

        builder.HasKey(dl => dl.Id);

        builder.Property(dl => dl.EntityName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(dl => dl.EntityId)
            .IsRequired();

        builder.Property(dl => dl.Reason)
            .IsRequired()
            .HasMaxLength(400);

        builder.Property(dl => dl.DeletedAt)
            .IsRequired();

        builder.Property(dl => dl.Metadata);

        // Index for entity lookups
        builder.HasIndex(dl => new { dl.EntityName, dl.EntityId });

        // Index for deletion reporting
        builder.HasIndex(dl => dl.DeletedAt);

        // Index for audit trail
        builder.HasIndex(dl => dl.DeletedBy);

        // Index for approval tracking
        builder.HasIndex(dl => dl.ApprovedBy);
    }
}
