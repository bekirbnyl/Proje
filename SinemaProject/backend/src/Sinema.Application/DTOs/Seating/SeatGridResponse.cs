namespace Sinema.Application.DTOs.Seating;

/// <summary>
/// Response DTO containing the seat grid status for a screening
/// </summary>
public class SeatGridResponse
{
    /// <summary>
    /// Unique identifier for the seat layout used in this screening
    /// </summary>
    public Guid SeatLayoutId { get; set; }

    /// <summary>
    /// Collection of seats with their current status for the screening
    /// </summary>
    public IEnumerable<SeatStatusDto> Seats { get; set; } = Enumerable.Empty<SeatStatusDto>();
}
