namespace Sinema.Application.DTOs.Seating;

/// <summary>
/// DTO representing the status of a seat for a specific screening
/// </summary>
public class SeatStatusDto
{
    /// <summary>
    /// Unique identifier for the seat
    /// </summary>
    public Guid SeatId { get; set; }

    /// <summary>
    /// Row number (1-based)
    /// </summary>
    public int Row { get; set; }

    /// <summary>
    /// Column number (1-based)
    /// </summary>
    public int Col { get; set; }

    /// <summary>
    /// Human-readable seat label (e.g., "A1", "B5")
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Current state of the seat for the screening
    /// Values: "available", "held", "reserved", "sold"
    /// </summary>
    public string State { get; set; } = string.Empty;
}
