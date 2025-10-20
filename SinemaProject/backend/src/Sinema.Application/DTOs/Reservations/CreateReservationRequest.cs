namespace Sinema.Application.DTOs.Reservations;

/// <summary>
/// Request DTO for creating a reservation
/// </summary>
public class CreateReservationRequest
{
    /// <summary>
    /// Screening ID for the reservation
    /// </summary>
    public Guid ScreeningId { get; set; }

    /// <summary>
    /// List of seat IDs to reserve
    /// </summary>
    public List<Guid> SeatIds { get; set; } = new();

    /// <summary>
    /// Client token for hold validation
    /// </summary>
    public string ClientToken { get; set; } = string.Empty;
}
