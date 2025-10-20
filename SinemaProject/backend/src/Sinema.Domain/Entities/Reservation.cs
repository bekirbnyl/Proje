using Sinema.Domain.Enums;

namespace Sinema.Domain.Entities;

/// <summary>
/// Represents a seat reservation for a specific screening
/// </summary>
public class Reservation
{
    /// <summary>
    /// Unique identifier for the reservation
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key to the screening
    /// </summary>
    public Guid ScreeningId { get; set; }

    /// <summary>
    /// Foreign key to the reserved seat
    /// </summary>
    public Guid SeatId { get; set; }

    /// <summary>
    /// Foreign key to the member who made the reservation (nullable for guest reservations)
    /// </summary>
    public Guid? MemberId { get; set; }

    /// <summary>
    /// Current status of the reservation
    /// </summary>
    public ReservationStatus Status { get; set; }

    /// <summary>
    /// When the reservation expires (T-30 minutes rule)
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// When the reservation was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Row version for optimistic concurrency control
    /// </summary>
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Navigation property to the screening
    /// </summary>
    public virtual Screening Screening { get; set; } = null!;

    /// <summary>
    /// Navigation property to the seat
    /// </summary>
    public virtual Seat Seat { get; set; } = null!;

    /// <summary>
    /// Navigation property to the member (if applicable)
    /// </summary>
    public virtual Member? Member { get; set; }

    /// <summary>
    /// Checks if the reservation has expired
    /// </summary>
    /// <param name="currentTime">Current time to check against</param>
    /// <returns>True if the reservation has expired</returns>
    public bool IsExpired(DateTime currentTime) => currentTime >= ExpiresAt && Status == ReservationStatus.Pending;

    /// <summary>
    /// Marks the reservation as expired
    /// </summary>
    public void MarkAsExpired()
    {
        Status = ReservationStatus.Expired;
    }

    /// <summary>
    /// Confirms the reservation
    /// </summary>
    public void Confirm()
    {
        if (Status != ReservationStatus.Pending)
        {
            throw new InvalidOperationException($"Cannot confirm reservation with status {Status}");
        }

        Status = ReservationStatus.Confirmed;
    }

    /// <summary>
    /// Cancels the reservation
    /// </summary>
    public void Cancel()
    {
        if (Status == ReservationStatus.Confirmed)
        {
            throw new InvalidOperationException("Cannot cancel a confirmed reservation");
        }

        Status = ReservationStatus.Canceled;
    }
}
