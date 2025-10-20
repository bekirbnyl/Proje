using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sinema.Domain.Entities;

namespace Sinema.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for Hall entity
/// </summary>
public class HallConfiguration : IEntityTypeConfiguration<Hall>
{
    public void Configure(EntityTypeBuilder<Hall> builder)
    {
        builder.ToTable("Halls");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(h => h.CreatedAt)
            
            .IsRequired();

        // Soft delete properties
        builder.Property(h => h.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(h => h.DeletedAt)
            ;

        builder.Property(h => h.DeletedBy);

        // Index for fast lookups by cinema and name
        builder.HasIndex(h => new { h.CinemaId, h.Name })
            .IsUnique();

        // Index for soft delete filtering
        builder.HasIndex(h => h.IsDeleted)
            .HasDatabaseName("IX_Halls_IsDeleted");

        // Index for deleted halls by date
        builder.HasIndex(h => h.DeletedAt)
            .HasDatabaseName("IX_Halls_DeletedAt");

        // Relationships
        builder.HasOne(h => h.Cinema)
            .WithMany(c => c.Halls)
            .HasForeignKey(h => h.CinemaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(h => h.SeatLayouts)
            .WithOne(sl => sl.Hall)
            .HasForeignKey(sl => sl.HallId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent deletion if active screenings exist

        builder.HasMany(h => h.Screenings)
            .WithOne(s => s.Hall)
            .HasForeignKey(s => s.HallId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
