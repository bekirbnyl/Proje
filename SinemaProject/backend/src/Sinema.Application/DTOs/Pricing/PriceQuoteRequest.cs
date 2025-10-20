using Sinema.Domain.Enums;

namespace Sinema.Application.DTOs.Pricing;

/// <summary>
/// Request for calculating a price quote
/// </summary>
public class PriceQuoteRequest
{
    /// <summary>
    /// ID of the screening for which to calculate prices
    /// </summary>
    public Guid ScreeningId { get; set; }

    /// <summary>
    /// ID of the member making the request (optional for anonymous pricing)
    /// Required for VIP-related discounts
    /// </summary>
    public Guid? MemberId { get; set; }

    /// <summary>
    /// Sales channel (optional, for future use)
    /// </summary>
    public TicketChannel? Channel { get; set; }

    /// <summary>
    /// List of items to price
    /// </summary>
    public List<QuoteItemRequest> Items { get; set; } = new();
}
