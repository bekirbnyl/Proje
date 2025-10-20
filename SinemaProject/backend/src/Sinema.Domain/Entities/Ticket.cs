using Sinema.Domain.Enums;

namespace Sinema.Domain.Entities;

/// <summary>
/// Represents a sold ticket for a specific seat and screening
/// </summary>
public class Ticket
{
    /// <summary>
    /// Unique identifier for the ticket
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key to the screening
    /// </summary>
    public Guid ScreeningId { get; set; }

    /// <summary>
    /// Foreign key to the seat
    /// </summary>
    public Guid SeatId { get; set; }

    /// <summary>
    /// Type of ticket (Full, Student, VIP, VIPGuest)
    /// </summary>
    public TicketType Type { get; set; }

    /// <summary>
    /// Channel through which the ticket was sold
    /// </summary>
    public TicketChannel Channel { get; set; }

    /// <summary>
    /// Final price paid for the ticket
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Foreign key to the payment (nullable for cash transactions)
    /// </summary>
    public Guid? PaymentId { get; set; }

    /// <summary>
    /// When the ticket was sold
    /// </summary>
    public DateTime SoldAt { get; set; }

    /// <summary>
    /// Unique ticket code for identification (e.g., "AB12-34CD")
    /// </summary>
    public string TicketCode { get; set; } = string.Empty;

    /// <summary>
    /// Applied pricing breakdown in JSON format
    /// </summary>
    public string? AppliedPricingJson { get; set; }

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
    /// Navigation property to the payment
    /// </summary>
    public virtual Payment? Payment { get; set; }

    /// <summary>
    /// Generates a unique ticket code for identification
    /// </summary>
    public string GenerateTicketCode()
    {
        return $"TK-{Id:N}".ToUpper()[..15];
    }

    /// <summary>
    /// Checks if this is a discounted ticket
    /// </summary>
    public bool IsDiscounted => Type != TicketType.Full;

    /// <summary>
    /// Gets the discount percentage based on ticket type
    /// </summary>
    public decimal GetDiscountPercentage()
    {
        return Type switch
        {
            TicketType.Student => 0.40m,
            TicketType.VIPGuest => 0.20m,
            _ => 0m
        };
    }
}
