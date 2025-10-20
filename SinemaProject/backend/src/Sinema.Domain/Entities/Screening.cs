namespace Sinema.Domain.Entities;

/// <summary>
/// Represents a scheduled movie screening in a specific hall at a specific time
/// </summary>
public class Screening
{
    /// <summary>
    /// Unique identifier for the screening
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key to the movie being screened
    /// </summary>
    public Guid MovieId { get; set; }

    /// <summary>
    /// Foreign key to the hall where the screening takes place
    /// </summary>
    public Guid HallId { get; set; }

    /// <summary>
    /// Foreign key to the seat layout version used for this screening
    /// </summary>
    public Guid SeatLayoutId { get; set; }

    /// <summary>
    /// Start time of the screening (UTC)
    /// </summary>
    public DateTime StartAt { get; set; }

    /// <summary>
    /// Duration of the screening in minutes
    /// </summary>
    public int DurationMinutes { get; set; }

    /// <summary>
    /// Indicates if this is the first weekday screening of the day in this hall
    /// </summary>
    public bool IsFirstShowWeekday { get; set; }

    /// <summary>
    /// Indicates if this screening is on a special day (e.g., Halk Günü)
    /// </summary>
    public bool IsSpecialDay { get; set; }

    /// <summary>
    /// When the screening record was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Whether the screening is soft-deleted
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// When the screening was soft-deleted (if applicable)
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// ID of the user who soft-deleted the screening
    /// </summary>
    public Guid? DeletedBy { get; set; }

    /// <summary>
    /// Navigation property to the movie
    /// </summary>
    public virtual Movie Movie { get; set; } = null!;

    /// <summary>
    /// Navigation property to the hall
    /// </summary>
    public virtual Hall Hall { get; set; } = null!;

    /// <summary>
    /// Navigation property to the seat layout
    /// </summary>
    public virtual SeatLayout SeatLayout { get; set; } = null!;

    /// <summary>
    /// Collection of reservations for this screening
    /// </summary>
    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    /// <summary>
    /// Collection of tickets sold for this screening
    /// </summary>
    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    /// <summary>
    /// Calculates the end time of the screening
    /// </summary>
    public DateTime EndAt => StartAt.AddMinutes(DurationMinutes);

    /// <summary>
    /// Checks if the screening conflicts with another screening in the same hall
    /// </summary>
    /// <param name="other">The other screening to check against</param>
    /// <returns>True if there is a conflict</returns>
    public bool ConflictsWith(Screening other)
    {
        if (HallId != other.HallId)
        {
            return false;
        }

        return StartAt < other.EndAt && EndAt > other.StartAt;
    }
}
