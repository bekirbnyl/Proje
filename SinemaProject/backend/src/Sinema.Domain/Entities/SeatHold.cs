namespace Sinema.Domain.Entities;

/// <summary>
/// Represents a temporary hold on a seat for a specific screening
/// Prevents other clients from reserving the seat until the hold expires or is released
/// </summary>
public class SeatHold
{
    /// <summary>
    /// Unique identifier for the seat hold
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key to the screening
    /// </summary>
    public Guid ScreeningId { get; set; }

    /// <summary>
    /// Foreign key to the held seat
    /// </summary>
    public Guid SeatId { get; set; }

    /// <summary>
    /// Client token identifying the browser/client that created this hold
    /// Used for ownership verification
    /// </summary>
    public string ClientToken { get; set; } = string.Empty;

    /// <summary>
    /// Optional user ID if the hold was created by an authenticated user
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// When the hold was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last time this hold was extended via heartbeat
    /// </summary>
    public DateTime LastHeartbeatAt { get; set; }

    /// <summary>
    /// When the hold expires and becomes available again
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Navigation property to the screening
    /// </summary>
    public virtual Screening Screening { get; set; } = null!;

    /// <summary>
    /// Navigation property to the seat
    /// </summary>
    public virtual Seat Seat { get; set; } = null!;

    /// <summary>
    /// Checks if the hold has expired
    /// </summary>
    /// <param name="currentTime">Current time to check against</param>
    /// <returns>True if the hold has expired</returns>
    public bool IsExpired(DateTime currentTime) => currentTime >= ExpiresAt;

    /// <summary>
    /// Checks if the provided client token owns this hold
    /// </summary>
    /// <param name="clientToken">Client token to verify</param>
    /// <param name="userId">Optional user ID to verify</param>
    /// <returns>True if the client owns this hold</returns>
    public bool IsOwnedBy(string clientToken, Guid? userId = null)
    {
        if (string.IsNullOrEmpty(clientToken) || !ClientToken.Equals(clientToken, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // If hold has a user ID, it must match the provided user ID
        if (UserId.HasValue && UserId != userId)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Extends the hold expiration time
    /// </summary>
    /// <param name="extensionSeconds">Number of seconds to extend</param>
    /// <param name="maxExpirationTime">Maximum allowed expiration time (e.g., T-30)</param>
    public void Extend(int extensionSeconds, DateTime? maxExpirationTime = null)
    {
        var newExpirationTime = DateTime.UtcNow.AddSeconds(extensionSeconds);
        
        if (maxExpirationTime.HasValue && newExpirationTime > maxExpirationTime.Value)
        {
            newExpirationTime = maxExpirationTime.Value;
        }

        ExpiresAt = newExpirationTime;
        LastHeartbeatAt = DateTime.UtcNow;
    }
}
