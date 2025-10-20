namespace Sinema.Application.DTOs.Reservations.Holds;

/// <summary>
/// Request DTO for releasing a seat hold
/// </summary>
public class ReleaseSeatHoldRequest
{
    /// <summary>
    /// Hold ID to release
    /// </summary>
    public Guid HoldId { get; set; }

    /// <summary>
    /// Client token for ownership verification
    /// </summary>
    public string ClientToken { get; set; } = string.Empty;
}
