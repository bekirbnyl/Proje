namespace Sinema.Application.DTOs.Reservations.Holds;

/// <summary>
/// Request DTO for extending a seat hold
/// </summary>
public class ExtendSeatHoldRequest
{
    /// <summary>
    /// Hold ID to extend
    /// </summary>
    public Guid HoldId { get; set; }

    /// <summary>
    /// Client token for ownership verification
    /// </summary>
    public string ClientToken { get; set; } = string.Empty;
}
