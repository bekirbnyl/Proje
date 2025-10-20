using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sinema.Infrastructure.Persistence.Entities;

namespace Sinema.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for ReportJob entity
/// </summary>
public class ReportJobConfiguration : IEntityTypeConfiguration<ReportJob>
{
    /// <summary>
    /// Configures the ReportJob entity
    /// </summary>
    /// <param name="builder">The entity type builder</param>
    public void Configure(EntityTypeBuilder<ReportJob> builder)
    {
        builder.ToTable("ReportJobs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .IsRequired()
            .ValueGeneratedNever();

        builder.Property(x => x.Type)
            .IsRequired()
            .HasComment("Report type: 1=Sales, 2=Deletions, 3=Memberships");

        builder.Property(x => x.Cron)
            .IsRequired()
            .HasMaxLength(100)
            .HasComment("Cron expression for scheduling");

        builder.Property(x => x.ParametersJson)
            .IsRequired()
            .HasMaxLength(2000)
            .HasComment("Report parameters in JSON format");

        builder.Property(x => x.OutputPath)
            .IsRequired()
            .HasMaxLength(500)
            .HasComment("Output path for generated reports");

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true)
            .HasComment("Whether the job is currently active");

        builder.Property(x => x.LastRunAt)
            .HasComment("When the job was last executed");

        builder.Property(x => x.LastStatus)
            .HasMaxLength(50)
            .HasComment("Status of the last execution");

        builder.Property(x => x.CreatedAt)
            .IsRequired()
            .HasComment("When the job was created");

        builder.Property(x => x.UpdatedAt)
            .HasComment("When the job was last updated");

        // Relationships
        builder.HasMany(x => x.Logs)
            .WithOne(x => x.Job)
            .HasForeignKey(x => x.JobId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.Type)
            .HasDatabaseName("IX_ReportJobs_Type");

        builder.HasIndex(x => x.IsActive)
            .HasDatabaseName("IX_ReportJobs_IsActive");

        builder.HasIndex(x => x.LastRunAt)
            .HasDatabaseName("IX_ReportJobs_LastRunAt");
    }
}
