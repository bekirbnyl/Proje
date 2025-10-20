namespace Sinema.Application.DTOs.Screenings;

/// <summary>
/// Simplified DTO for screening list items
/// </summary>
public class ScreeningListItemDto
{
    /// <summary>
    /// Unique identifier for the screening
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Movie ID
    /// </summary>
    public Guid MovieId { get; set; }

    /// <summary>
    /// Hall ID
    /// </summary>
    public Guid HallId { get; set; }

    /// <summary>
    /// Movie title
    /// </summary>
    public string MovieTitle { get; set; } = string.Empty;

    /// <summary>
    /// Hall name
    /// </summary>
    public string HallName { get; set; } = string.Empty;

    /// <summary>
    /// Cinema name
    /// </summary>
    public string CinemaName { get; set; } = string.Empty;

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
    /// Calculates the end time of the screening
    /// </summary>
    public DateTime EndAt => StartAt.AddMinutes(DurationMinutes);
}
