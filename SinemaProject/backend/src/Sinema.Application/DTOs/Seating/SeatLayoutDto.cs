namespace Sinema.Application.DTOs.Seating;

/// <summary>
/// DTO representing a seat layout version
/// </summary>
public class SeatLayoutDto
{
    /// <summary>
    /// Unique identifier for the seat layout
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key to the hall this layout belongs to
    /// </summary>
    public Guid HallId { get; set; }

    /// <summary>
    /// Version number of this layout (incremental)
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// Whether this layout version is currently active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// When this layout version was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Collection of seats in this layout
    /// </summary>
    public IEnumerable<SeatDto> Seats { get; set; } = Enumerable.Empty<SeatDto>();
}

/// <summary>
/// DTO representing a seat within a layout
/// </summary>
public class SeatDto
{
    /// <summary>
    /// Unique identifier for the seat
    /// </summary>
    public Guid Id { get; set; }

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
}
