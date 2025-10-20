using System.ComponentModel.DataAnnotations;

namespace Sinema.Application.DTOs.Movies;

/// <summary>
/// Data transfer object for Movie entity
/// </summary>
public class MovieDto
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
}

/// <summary>
/// Request DTO for creating a new movie
/// </summary>
public class CreateMovieRequest
{
    /// <summary>
    /// Title of the movie
    /// </summary>
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Duration of the movie in minutes
    /// </summary>
    [Range(1, 600, ErrorMessage = "Duration must be between 1 and 600 minutes")]
    public int DurationMinutes { get; set; }

    /// <summary>
    /// Whether the movie is currently active for screening (defaults to true)
    /// </summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Request DTO for updating an existing movie
/// </summary>
public class UpdateMovieRequest
{
    /// <summary>
    /// Title of the movie
    /// </summary>
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Duration of the movie in minutes
    /// </summary>
    [Range(1, 600, ErrorMessage = "Duration must be between 1 and 600 minutes")]
    public int DurationMinutes { get; set; }

    /// <summary>
    /// Whether the movie is currently active for screening
    /// </summary>
    public bool IsActive { get; set; }
}
