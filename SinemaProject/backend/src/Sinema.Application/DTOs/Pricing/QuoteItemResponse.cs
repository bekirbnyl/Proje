namespace Sinema.Application.DTOs.Pricing;

/// <summary>
/// Represents a priced item in the quote response
/// </summary>
public class QuoteItemResponse
{
    /// <summary>
    /// ID of the specific seat (if specified in request)
    /// </summary>
    public Guid? SeatId { get; set; }

    /// <summary>
    /// Base price before any discounts
    /// </summary>
    public decimal BasePrice { get; set; }

    /// <summary>
    /// Final price after applying discounts
    /// </summary>
    public decimal FinalPrice { get; set; }

    /// <summary>
    /// Information about the pricing rule that was applied
    /// </summary>
    public AppliedRuleDto AppliedRule { get; set; } = null!;

    /// <summary>
    /// Quantity of tickets for this item
    /// </summary>
    public int Quantity { get; set; } = 1;

    /// <summary>
    /// Total base price for this item (BasePrice * Quantity)
    /// </summary>
    public decimal TotalBasePrice => BasePrice * Quantity;

    /// <summary>
    /// Total final price for this item (FinalPrice * Quantity)
    /// </summary>
    public decimal TotalFinalPrice => FinalPrice * Quantity;

    /// <summary>
    /// Total discount amount for this item
    /// </summary>
    public decimal TotalDiscount => TotalBasePrice - TotalFinalPrice;
}
