using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sinema.Domain.Entities;

namespace Sinema.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for SeatLayout entity
/// </summary>
public class SeatLayoutConfiguration : IEntityTypeConfiguration<SeatLayout>
{
    public void Configure(EntityTypeBuilder<SeatLayout> builder)
    {
        builder.ToTable("SeatLayouts");

        builder.HasKey(sl => sl.Id);

        builder.Property(sl => sl.Version)
            .IsRequired();

        builder.Property(sl => sl.IsActive)
            .IsRequired();

        builder.Property(sl => sl.CreatedAt)
            
            .IsRequired();

        // Soft delete properties
        builder.Property(sl => sl.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(sl => sl.DeletedAt)
            ;

        builder.Property(sl => sl.DeletedBy);

        // Index for version lookups
        builder.HasIndex(sl => new { sl.HallId, sl.Version })
            .IsUnique();

        // Index for active layout lookups
        builder.HasIndex(sl => new { sl.HallId, sl.IsActive });

        // Index for soft delete filtering
        builder.HasIndex(sl => sl.IsDeleted)
            .HasDatabaseName("IX_SeatLayouts_IsDeleted");

        // Index for deleted seat layouts by date
        builder.HasIndex(sl => sl.DeletedAt)
            .HasDatabaseName("IX_SeatLayouts_DeletedAt");

        // Relationships
        builder.HasOne(sl => sl.Hall)
            .WithMany(h => h.SeatLayouts)
            .HasForeignKey(sl => sl.HallId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(sl => sl.Seats)
            .WithOne(s => s.SeatLayout)
            .HasForeignKey(s => s.SeatLayoutId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(sl => sl.Screenings)
            .WithOne(s => s.SeatLayout)
            .HasForeignKey(s => s.SeatLayoutId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
