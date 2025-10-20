namespace Sinema.Domain.Entities;

/// <summary>
/// Represents a movie that can be screened
/// </summary>
public class Movie
{
    /// <summary>
    /// Unique identifier for the movie
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Title of the movie
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Duration of the movie in minutes
    /// </summary>
    public int DurationMinutes { get; set; }

    /// <summary>
    /// Whether the movie is currently active for screening
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// When the movie record was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Whether the movie is soft-deleted
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// When the movie was soft-deleted (if applicable)
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// ID of the user who soft-deleted the movie
    /// </summary>
    public Guid? DeletedBy { get; set; }

    /// <summary>
    /// Collection of screenings for this movie
    /// </summary>
    public virtual ICollection<Screening> Screenings { get; set; } = new List<Screening>();
}
