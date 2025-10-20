namespace Sinema.Domain.Entities;

/// <summary>
/// Represents a versioned seat layout configuration for a hall
/// </summary>
public class SeatLayout
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
    /// Whether the seat layout is soft-deleted
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// When the seat layout was soft-deleted (if applicable)
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// ID of the user who soft-deleted the seat layout
    /// </summary>
    public Guid? DeletedBy { get; set; }

    /// <summary>
    /// Navigation property to the hall
    /// </summary>
    public virtual Hall Hall { get; set; } = null!;

    /// <summary>
    /// Collection of seats in this layout
    /// </summary>
    public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();

    /// <summary>
    /// Collection of screenings using this layout
    /// </summary>
    public virtual ICollection<Screening> Screenings { get; set; } = new List<Screening>();
}
