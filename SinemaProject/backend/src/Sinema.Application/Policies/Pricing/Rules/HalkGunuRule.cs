using Sinema.Application.Abstractions.Pricing;
using Sinema.Application.DTOs.Pricing;

namespace Sinema.Application.Policies.Pricing.Rules;

/// <summary>
/// Pricing rule for Halk Günü (Special Day) - 50% discount
/// Applies when screening IsSpecialDay is true or current day matches HalkGunu setting
/// </summary>
public class HalkGunuRule : IPricingRule
{
    public string RuleCode => "HALK_GUNU_50";
    public string RuleTitle => "Halk Günü %50 İndirim";
    public int ExecutionOrder => 10; // After VIP free, but high priority

    public Task<bool> IsApplicableAsync(PricingContext context, QuoteItemRequest item)
    {
        // Halk Günü discount applies to all ticket types
        // Check if today is Halk Günü
        return Task.FromResult(context.IsHalkGunu);
    }

    public Task<AppliedRuleDto> CalculateDiscountAsync(PricingContext context, QuoteItemRequest item, decimal basePrice)
    {
        // 50% discount on Halk Günü
        const decimal discountPercentage = 0.50m;
        
        var appliedRule = AppliedRuleDto.CreatePercentageDiscount(
            RuleCode,
            RuleTitle,
            basePrice,
            discountPercentage);

        // Add details about why this rule was applied
        if (context.Screening.IsSpecialDay)
        {
            appliedRule.Details = "Özel gün olarak işaretlenmiş seans - %50 indirim";
        }
        else
        {
            appliedRule.Details = $"Halk Günü ({context.HalkGunuSetting}) - %50 indirim";
        }

        return Task.FromResult(appliedRule);
    }
}
