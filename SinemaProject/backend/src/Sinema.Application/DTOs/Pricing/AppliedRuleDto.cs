namespace Sinema.Application.DTOs.Pricing;

/// <summary>
/// Represents a pricing rule that has been applied to an item
/// </summary>
public class AppliedRuleDto
{
    /// <summary>
    /// Unique code identifying the applied rule
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable title of the applied rule
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Amount of discount applied (positive value)
    /// </summary>
    public decimal AmountOff { get; set; }

    /// <summary>
    /// Final price after applying this rule
    /// </summary>
    public decimal FinalPrice { get; set; }

    /// <summary>
    /// Additional details about the rule application (optional)
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Creates an applied rule for a free ticket (VIP monthly free)
    /// </summary>
    /// <param name="basePrice">Original price</param>
    /// <returns>Applied rule with full discount</returns>
    public static AppliedRuleDto CreateFreeTicket(decimal basePrice)
    {
        return new AppliedRuleDto
        {
            Code = "VIP_MONTHLY_FREE",
            Title = "VIP Aylık Ücretsiz Bilet",
            AmountOff = basePrice,
            FinalPrice = 0m,
            Details = "VIP üyelik aylık ücretsiz bilet hakkı"
        };
    }

    /// <summary>
    /// Creates an applied rule for a percentage discount
    /// </summary>
    /// <param name="code">Rule code</param>
    /// <param name="title">Rule title</param>
    /// <param name="basePrice">Original price</param>
    /// <param name="discountPercentage">Discount percentage (0.0 to 1.0)</param>
    /// <returns>Applied rule with percentage discount</returns>
    public static AppliedRuleDto CreatePercentageDiscount(string code, string title, decimal basePrice, decimal discountPercentage)
    {
        var amountOff = basePrice * discountPercentage;
        return new AppliedRuleDto
        {
            Code = code,
            Title = title,
            AmountOff = amountOff,
            FinalPrice = basePrice - amountOff,
            Details = $"{discountPercentage:P0} indirim"
        };
    }

    /// <summary>
    /// Creates a no-discount rule (base price)
    /// </summary>
    /// <param name="basePrice">Base price</param>
    /// <returns>Applied rule with no discount</returns>
    public static AppliedRuleDto CreateNoDiscount(decimal basePrice)
    {
        return new AppliedRuleDto
        {
            Code = "BASE_PRICE",
            Title = "Normal Fiyat",
            AmountOff = 0m,
            FinalPrice = basePrice,
            Details = "İndirim uygulanmadı"
        };
    }
}
