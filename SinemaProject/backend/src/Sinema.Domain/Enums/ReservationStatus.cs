namespace Sinema.Domain.Enums;

/// <summary>
/// Represents the current status of a reservation
/// </summary>
public enum ReservationStatus
{
    /// <summary>
    /// Reservation is pending confirmation and payment
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Reservation has been confirmed with payment
    /// </summary>
    Confirmed = 1,

    /// <summary>
    /// Reservation has expired (T-30 minutes rule)
    /// </summary>
    Expired = 2,

    /// <summary>
    /// Reservation was manually canceled
    /// </summary>
    Canceled = 3,

    /// <summary>
    /// Reservation has been completed with ticket purchase
    /// </summary>
    Completed = 4
}
