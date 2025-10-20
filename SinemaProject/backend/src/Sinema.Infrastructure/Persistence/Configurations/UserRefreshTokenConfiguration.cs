using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sinema.Infrastructure.Persistence.Entities;

namespace Sinema.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for UserRefreshToken entity
/// </summary>
public class UserRefreshTokenConfiguration : IEntityTypeConfiguration<UserRefreshToken>
{
    public void Configure(EntityTypeBuilder<UserRefreshToken> builder)
    {
        builder.ToTable("UserRefreshTokens");

        builder.HasKey(urt => urt.Id);

        builder.Property(urt => urt.UserId)
            .HasMaxLength(450) // Standard length for ASP.NET Identity keys
            .IsRequired();

        builder.Property(urt => urt.Token)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(urt => urt.CreatedAt)
            
            .IsRequired();

        builder.Property(urt => urt.ExpiresAt)
            
            .IsRequired();

        builder.Property(urt => urt.RevokedAt)
            ;

        builder.Property(urt => urt.DeviceInfo)
            .HasMaxLength(200);

        // Index for user lookups
        builder.HasIndex(urt => urt.UserId)
            .HasDatabaseName("IX_UserRefreshTokens_UserId");

        // Unique index for token value (prevents token reuse)
        builder.HasIndex(urt => urt.Token)
            .IsUnique()
            .HasDatabaseName("IX_UserRefreshTokens_Token_Unique");

        // Index for cleanup of expired tokens
        builder.HasIndex(urt => new { urt.ExpiresAt, urt.RevokedAt })
            .HasDatabaseName("IX_UserRefreshTokens_Cleanup");

        // Foreign key to AspNetUsers
        builder.HasOne<Sinema.Infrastructure.Identity.ApplicationUser>()
            .WithMany()
            .HasForeignKey(urt => urt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
