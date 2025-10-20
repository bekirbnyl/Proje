namespace Sinema.Domain.Entities;

/// <summary>
/// Represents a cinema location with multiple halls
/// </summary>
public class Cinema
{
    /// <summary>
    /// Unique identifier for the cinema
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Name of the cinema
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// When the cinema record was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Collection of halls in this cinema
    /// </summary>
    public virtual ICollection<Hall> Halls { get; set; } = new List<Hall>();
}
