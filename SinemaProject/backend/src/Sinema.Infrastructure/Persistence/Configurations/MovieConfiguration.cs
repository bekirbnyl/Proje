using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sinema.Domain.Entities;

namespace Sinema.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for Movie entity
/// </summary>
public class MovieConfiguration : IEntityTypeConfiguration<Movie>
{
    public void Configure(EntityTypeBuilder<Movie> builder)
    {
        builder.ToTable("Movies");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.DurationMinutes)
            .IsRequired();

        builder.Property(m => m.IsActive)
            .IsRequired();

        builder.Property(m => m.CreatedAt)
            
            .IsRequired();

        // Soft delete properties
        builder.Property(m => m.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(m => m.DeletedAt)
            ;

        builder.Property(m => m.DeletedBy);

        // Index for fast title lookups
        builder.HasIndex(m => m.Title);

        // Index for active movies
        builder.HasIndex(m => m.IsActive);

        // Index for soft delete filtering
        builder.HasIndex(m => m.IsDeleted)
            .HasDatabaseName("IX_Movies_IsDeleted");

        // Index for deleted movies by date
        builder.HasIndex(m => m.DeletedAt)
            .HasDatabaseName("IX_Movies_DeletedAt");

        // Relationships
        builder.HasMany(m => m.Screenings)
            .WithOne(s => s.Movie)
            .HasForeignKey(s => s.MovieId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
