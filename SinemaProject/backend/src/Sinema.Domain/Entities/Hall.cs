namespace Sinema.Domain.Entities;

/// <summary>
/// Represents a screening hall within a cinema
/// </summary>
public class Hall
{
    /// <summary>
    /// Unique identifier for the hall
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key to the cinema this hall belongs to
    /// </summary>
    public Guid CinemaId { get; set; }

    /// <summary>
    /// Name of the hall
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// When the hall record was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Whether the hall is soft-deleted
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// When the hall was soft-deleted (if applicable)
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// ID of the user who soft-deleted the hall
    /// </summary>
    public Guid? DeletedBy { get; set; }

    /// <summary>
    /// Navigation property to the cinema
    /// </summary>
    public virtual Cinema Cinema { get; set; } = null!;

    /// <summary>
    /// Collection of seat layouts for this hall (versioned)
    /// </summary>
    public virtual ICollection<SeatLayout> SeatLayouts { get; set; } = new List<SeatLayout>();

    /// <summary>
    /// Collection of screenings in this hall
    /// </summary>
    public virtual ICollection<Screening> Screenings { get; set; } = new List<Screening>();
}
