namespace Sinema.Application.DTOs.Reservations.Holds;

/// <summary>
/// Request DTO for creating seat holds
/// </summary>
public class CreateSeatHoldsRequest
{
    /// <summary>
    /// Screening ID for which to create holds
    /// </summary>
    public Guid ScreeningId { get; set; }

    /// <summary>
    /// List of seat IDs to hold
    /// </summary>
    public List<Guid> SeatIds { get; set; } = new();

    /// <summary>
    /// Client token identifying the browser/client
    /// </summary>
    public string ClientToken { get; set; } = string.Empty;

    /// <summary>
    /// Optional custom TTL in seconds (uses default if not provided)
    /// </summary>
    public int? TtlSeconds { get; set; }
}
