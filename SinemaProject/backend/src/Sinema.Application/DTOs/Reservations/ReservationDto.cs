using Sinema.Domain.Enums;

namespace Sinema.Application.DTOs.Reservations;

/// <summary>
/// DTO representing a reservation
/// </summary>
public class ReservationDto
{
    /// <summary>
    /// Reservation ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Screening ID
    /// </summary>
    public Guid ScreeningId { get; set; }

    /// <summary>
    /// Seat ID
    /// </summary>
    public Guid SeatId { get; set; }

    /// <summary>
    /// Member ID (nullable for guest reservations)
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
    /// Seat information
    /// </summary>
    public ReservationSeatInfo? Seat { get; set; }
}

/// <summary>
/// Basic seat information for reservation DTOs
/// </summary>
public class ReservationSeatInfo
{
    /// <summary>
    /// Row number
    /// </summary>
    public int Row { get; set; }

    /// <summary>
    /// Column number
    /// </summary>
    public int Col { get; set; }

    /// <summary>
    /// Seat label (e.g., "A1")
    /// </summary>
    public string Label { get; set; } = string.Empty;
}
