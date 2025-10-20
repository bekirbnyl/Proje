namespace Sinema.Application.DTOs.Seating;

/// <summary>
/// Request DTO for adding a new seat layout version to a hall
/// </summary>
public class AddSeatLayoutVersionRequest
{
    /// <summary>
    /// Collection of seats to be created in the new layout version
    /// </summary>
    public IEnumerable<CreateSeatDto> Seats { get; set; } = Enumerable.Empty<CreateSeatDto>();
}

/// <summary>
/// DTO for creating a new seat
/// </summary>
public class CreateSeatDto
{
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
}
