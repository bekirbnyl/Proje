using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sinema.Infrastructure.Persistence.Entities;

namespace Sinema.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for ReportLog entity
/// </summary>
public class ReportLogConfiguration : IEntityTypeConfiguration<ReportLog>
{
    /// <summary>
    /// Configures the ReportLog entity
    /// </summary>
    /// <param name="builder">The entity type builder</param>
    public void Configure(EntityTypeBuilder<ReportLog> builder)
    {
        builder.ToTable("ReportLogs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired()
            .ValueGeneratedNever();

        builder.Property(x => x.JobId)
            .IsRequired()
            .HasComment("Foreign key to the report job");

        builder.Property(x => x.StartedAt)
            .IsRequired()
            .HasComment("When the report generation started");

        builder.Property(x => x.FinishedAt)
            .HasComment("When the report generation finished");

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(50)
            .HasComment("Status of the report generation");

        builder.Property(x => x.Message)
            .HasMaxLength(2000)
            .HasComment("Additional message or error details");

        builder.Property(x => x.OutputFilePath)
            .HasMaxLength(1000)
            .HasComment("Path to the generated output file");

        builder.Property(x => x.FileSizeBytes)
            .HasComment("Size of the generated file in bytes");

        builder.Property(x => x.RowCount)
            .HasComment("Number of rows in the generated report");

        // Relationships
        builder.HasOne(x => x.Job)
            .WithMany(x => x.Logs)
            .HasForeignKey(x => x.JobId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.JobId)
            .HasDatabaseName("IX_ReportLogs_JobId");

        builder.HasIndex(x => x.StartedAt)
            .HasDatabaseName("IX_ReportLogs_StartedAt");

        builder.HasIndex(x => x.Status)
            .HasDatabaseName("IX_ReportLogs_Status");

        builder.HasIndex(x => new { x.JobId, x.StartedAt })
            .HasDatabaseName("IX_ReportLogs_JobId_StartedAt");
    }
}
