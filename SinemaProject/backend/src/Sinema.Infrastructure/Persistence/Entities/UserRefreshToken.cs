namespace Sinema.Infrastructure.Persistence.Entities;

/// <summary>
/// Represents a refresh token for a user with rotation and revocation support
/// </summary>
public class UserRefreshToken
{
    /// <summary>
    /// Unique identifier for the refresh token
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key to the user this token belongs to
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// The refresh token value (cryptographically secure random string)
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// When the token was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the token expires
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// When the token was revoked (null if still active)
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// Device/client information for audit purposes
    /// </summary>
    public string? DeviceInfo { get; set; }

    /// <summary>
    /// Checks if the token is currently active (not expired and not revoked)
    /// </summary>
    public bool IsActive => RevokedAt == null && DateTime.UtcNow < ExpiresAt;

    /// <summary>
    /// Checks if the token has expired
    /// </summary>
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    /// <summary>
    /// Revokes the token
    /// </summary>
    public void Revoke()
    {
        RevokedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a new refresh token
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="token">Token value</param>
    /// <param name="lifetimeDays">Token lifetime in days</param>
    /// <param name="deviceInfo">Device information</param>
    /// <returns>UserRefreshToken instance</returns>
    public static UserRefreshToken Create(string userId, string token, int lifetimeDays, string? deviceInfo = null)
    {
        return new UserRefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(lifetimeDays),
            DeviceInfo = deviceInfo
        };
    }
}
