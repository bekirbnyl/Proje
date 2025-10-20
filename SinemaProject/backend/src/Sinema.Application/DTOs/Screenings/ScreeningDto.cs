namespace Sinema.Application.DTOs.Screenings;

/// <summary>
/// Data transfer object for Screening entity
/// </summary>
public class ScreeningDto
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
    /// Movie title (navigation property)
    /// </summary>
    public string? MovieTitle { get; set; }

    /// <summary>
    /// Hall name (navigation property)
    /// </summary>
    public string? HallName { get; set; }

    /// <summary>
    /// Calculates the end time of the screening
    /// </summary>
    public DateTime EndAt => StartAt.AddMinutes(DurationMinutes);
}
