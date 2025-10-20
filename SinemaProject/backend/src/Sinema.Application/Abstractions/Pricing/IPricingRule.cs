using Sinema.Application.DTOs.Pricing;
using Sinema.Application.Policies.Pricing;

namespace Sinema.Application.Abstractions.Pricing;

/// <summary>
/// Represents a pricing rule that can be applied to a ticket item
/// </summary>
public interface IPricingRule
{
    /// <summary>
    /// Unique code for this pricing rule
    /// </summary>
    string RuleCode { get; }

    /// <summary>
    /// Human-readable title for this pricing rule
    /// </summary>
    string RuleTitle { get; }

    /// <summary>
    /// Order of execution (lower values execute first)
    /// VipMonthlyFree should have the lowest order (e.g., 1)
    /// Other rules can have higher orders (e.g., 10-50)
    /// </summary>
    int ExecutionOrder { get; }

    /// <summary>
    /// Determines if this rule is applicable to the given context and item
    /// </summary>
    /// <param name="context">Pricing context containing screening, member info, etc.</param>
    /// <param name="item">The specific item being priced</param>
    /// <returns>True if this rule can be applied</returns>
    Task<bool> IsApplicableAsync(PricingContext context, QuoteItemRequest item);

    /// <summary>
    /// Calculates the discount amount for the given context and item
    /// </summary>
    /// <param name="context">Pricing context</param>
    /// <param name="item">The specific item being priced</param>
    /// <param name="basePrice">The base price before any discounts</param>
    /// <returns>Applied rule information with discount amount</returns>
    Task<AppliedRuleDto> CalculateDiscountAsync(PricingContext context, QuoteItemRequest item, decimal basePrice);
}
