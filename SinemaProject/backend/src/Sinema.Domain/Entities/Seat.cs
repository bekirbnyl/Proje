using Sinema.Domain.ValueObjects;

namespace Sinema.Domain.Entities;

/// <summary>
/// Represents an individual seat within a seat layout
/// </summary>
public class Seat
{
    /// <summary>
    /// Unique identifier for the seat
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key to the seat layout this seat belongs to
    /// </summary>
    public Guid SeatLayoutId { get; set; }

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
    /// Navigation property to the seat layout
    /// </summary>
    public virtual SeatLayout SeatLayout { get; set; } = null!;

    /// <summary>
    /// Collection of reservations for this seat
    /// </summary>
    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    /// <summary>
    /// Collection of tickets sold for this seat
    /// </summary>
    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    /// <summary>
    /// Gets the position of this seat as a value object
    /// </summary>
    public SeatPosition Position => new(Row, Col);

    /// <summary>
    /// Generates the seat label from row and column
    /// </summary>
    public void GenerateLabel()
    {
        Label = Position.ToLabel();
    }
}
