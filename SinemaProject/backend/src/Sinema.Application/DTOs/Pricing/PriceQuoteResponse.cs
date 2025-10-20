namespace Sinema.Application.DTOs.Pricing;

/// <summary>
/// Response containing the calculated price quote
/// </summary>
public class PriceQuoteResponse
{
    /// <summary>
    /// Currency of all prices (e.g., "TRY")
    /// </summary>
    public string Currency { get; set; } = "TRY";

    /// <summary>
    /// Total price before any discounts
    /// </summary>
    public decimal TotalBefore { get; set; }

    /// <summary>
    /// Total price after applying all discounts
    /// </summary>
    public decimal TotalAfter { get; set; }

    /// <summary>
    /// Total discount amount applied
    /// </summary>
    public decimal TotalDiscount => TotalBefore - TotalAfter;

    /// <summary>
    /// Breakdown of individual items with their pricing
    /// </summary>
    public List<QuoteItemResponse> Items { get; set; } = new();

    /// <summary>
    /// Summary of unique rules applied across all items
    /// </summary>
    public List<string> AppliedRulesSummary => Items
        .Select(i => i.AppliedRule.Title)
        .Distinct()
        .ToList();

    /// <summary>
    /// Indicates if any VIP benefits were applied
    /// </summary>
    public bool HasVipBenefits => Items.Any(i => 
        i.AppliedRule.Code.StartsWith("VIP_"));

    /// <summary>
    /// Indicates if any discount was applied
    /// </summary>
    public bool HasDiscounts => TotalDiscount > 0;
}
