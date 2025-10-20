using Sinema.Domain.Enums;

namespace Sinema.Application.DTOs.Pricing;

/// <summary>
/// Represents a single item in a pricing quote request
/// </summary>
public class QuoteItemRequest
{
    /// <summary>
    /// ID of the specific seat (optional for general pricing)
    /// </summary>
    public Guid? SeatId { get; set; }

    /// <summary>
    /// Type of ticket being requested
    /// </summary>
    public TicketType TicketType { get; set; }

    /// <summary>
    /// Indicates if this item is for a VIP guest
    /// Used when TicketType is not VIPGuest but the item should receive VIP guest discount
    /// </summary>
    public bool IsVipGuest { get; set; }

    /// <summary>
    /// Quantity of tickets for this item (default: 1)
    /// </summary>
    public int Quantity { get; set; } = 1;
}
