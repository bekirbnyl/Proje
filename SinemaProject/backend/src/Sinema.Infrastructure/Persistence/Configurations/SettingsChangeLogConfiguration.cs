using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sinema.Domain.Entities;

namespace Sinema.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for SettingsChangeLog entity
/// </summary>
public class SettingsChangeLogConfiguration : IEntityTypeConfiguration<SettingsChangeLog>
{
    public void Configure(EntityTypeBuilder<SettingsChangeLog> builder)
    {
        builder.ToTable("SettingsChangeLogs");

        builder.HasKey(scl => scl.Id);

        builder.Property(scl => scl.Key)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(scl => scl.OldValue);

        builder.Property(scl => scl.NewValue)
            .IsRequired();

        builder.Property(scl => scl.ChangedBy)
            .IsRequired(false);

        builder.Property(scl => scl.ChangedAt)
            .IsRequired();

        builder.Property(scl => scl.Metadata);

        // Indexes for efficient querying
        builder.HasIndex(scl => scl.Key)
            .HasDatabaseName("IX_SettingsChangeLogs_Key");

        builder.HasIndex(scl => scl.ChangedBy)
            .HasDatabaseName("IX_SettingsChangeLogs_ChangedBy");

        builder.HasIndex(scl => scl.ChangedAt)
            .HasDatabaseName("IX_SettingsChangeLogs_ChangedAt");

        // Composite index for key + date range queries
        builder.HasIndex(scl => new { scl.Key, scl.ChangedAt })
            .HasDatabaseName("IX_SettingsChangeLogs_Key_ChangedAt");
    }
}
