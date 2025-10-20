using Sinema.Application.Abstractions.Pricing;
using Sinema.Application.DTOs.Pricing;

namespace Sinema.Application.Policies.Pricing.Rules;

/// <summary>
/// Pricing rule for first weekday show - 50% discount
/// Applies when screening IsFirstShowWeekday is true
/// </summary>
public class FirstWeekdayShowRule : IPricingRule
{
    public string RuleCode => "FIRST_SHOW_50";
    public string RuleTitle => "Hafta İçi İlk Seans %50 İndirim";
    public int ExecutionOrder => 11; // After VIP free, same level as Halk Günü

    public Task<bool> IsApplicableAsync(PricingContext context, QuoteItemRequest item)
    {
        // First weekday show discount applies to all ticket types
        // Check if this is marked as first weekday show
        return Task.FromResult(context.IsFirstWeekdayShow);
    }

    public Task<AppliedRuleDto> CalculateDiscountAsync(PricingContext context, QuoteItemRequest item, decimal basePrice)
    {
        // 50% discount for first weekday show
        const decimal discountPercentage = 0.50m;
        
        var appliedRule = AppliedRuleDto.CreatePercentageDiscount(
            RuleCode,
            RuleTitle,
            basePrice,
            discountPercentage);

        appliedRule.Details = "Hafta içi günün ilk seansı - %50 indirim";

        return Task.FromResult(appliedRule);
    }
}
