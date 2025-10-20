using System.ComponentModel.DataAnnotations;

namespace Sinema.Application.DTOs.Screenings;

/// <summary>
/// Request DTO for updating an existing screening
/// </summary>
public class UpdateScreeningRequest
{
    /// <summary>
    /// Foreign key to the movie being screened
    /// </summary>
    [Required]
    public Guid MovieId { get; set; }

    /// <summary>
    /// Foreign key to the hall where the screening takes place
    /// </summary>
    [Required]
    public Guid HallId { get; set; }

    /// <summary>
    /// Start time of the screening (UTC)
    /// </summary>
    [Required]
    public DateTime StartAt { get; set; }

    /// <summary>
    /// Duration of the screening in minutes
    /// </summary>
    [Required]
    [Range(1, 600, ErrorMessage = "Duration must be between 1 and 600 minutes")]
    public int DurationMinutes { get; set; }

    /// <summary>
    /// Foreign key to the seat layout version (optional - will use hall's active layout if not provided)
    /// </summary>
    public Guid? SeatLayoutId { get; set; }
}
