using Sinema.Domain.Enums;

namespace Sinema.Application.DTOs.Tickets;

/// <summary>
/// Request for selling tickets - supports both reservation-based and direct sales
/// </summary>
public class SellTicketRequest
{
    /// <summary>
    /// ID of the screening for which tickets are being sold
    /// </summary>
    public Guid ScreeningId { get; set; }

    /// <summary>
    /// ID of the reservation (for web-based sales from existing reservation)
    /// Mutually exclusive with Items - one must be provided
    /// </summary>
    public Guid? ReservationId { get; set; }

    /// <summary>
    /// ID of the member purchasing tickets (optional for guest purchases)
    /// </summary>
    public Guid? MemberId { get; set; }

    /// <summary>
    /// Channel through which the sale is being made
    /// </summary>
    public TicketChannel Channel { get; set; }

    /// <summary>
    /// Payment method for the transaction
    /// </summary>
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>
    /// Client token for web-based transactions (required for reservation-based sales)
    /// </summary>
    public string? ClientToken { get; set; }

    /// <summary>
    /// Items to purchase (for direct box office sales)
    /// Mutually exclusive with ReservationId - one must be provided
    /// </summary>
    public List<SellTicketItemRequest>? Items { get; set; }

    /// <summary>
    /// Idempotency key to prevent duplicate transactions
    /// </summary>
    public string? IdempotencyKey { get; set; }
}
