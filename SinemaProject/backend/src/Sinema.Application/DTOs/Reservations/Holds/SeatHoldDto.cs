namespace Sinema.Application.DTOs.Reservations.Holds;

/// <summary>
/// DTO representing a seat hold
/// </summary>
public class SeatHoldDto
{
    /// <summary>
    /// Hold ID
    /// </summary>
    public Guid HoldId { get; set; }

    /// <summary>
    /// Seat ID that is held
    /// </summary>
    public Guid SeatId { get; set; }

    /// <summary>
    /// Screening ID
    /// </summary>
    public Guid ScreeningId { get; set; }

    /// <summary>
    /// When the hold expires
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Client token that owns this hold
    /// </summary>
    public string ClientToken { get; set; } = string.Empty;

    /// <summary>
    /// When the hold was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last heartbeat time
    /// </summary>
    public DateTime LastHeartbeatAt { get; set; }

    /// <summary>
    /// Seat information (optional)
    /// </summary>
    public HoldSeatInfo? Seat { get; set; }
}

/// <summary>
/// Basic seat information for hold DTOs
/// </summary>
public class HoldSeatInfo
{
    /// <summary>
    /// Row number
    /// </summary>
    public int Row { get; set; }

    /// <summary>
    /// Column number
    /// </summary>
    public int Col { get; set; }

    /// <summary>
    /// Seat label (e.g., "A1")
    /// </summary>
    public string Label { get; set; } = string.Empty;
}
