using Sinema.Application.Abstractions.Pricing;
using Sinema.Application.DTOs.Pricing;
using Sinema.Domain.Enums;

namespace Sinema.Application.Policies.Pricing.Rules;

/// <summary>
/// Pricing rule for VIP additional movies after monthly free movie
/// VIP members pay Halk Günü price (50% discount) for additional movies
/// </summary>
public class VipAdditionalMovieRule : IPricingRule
{
    public string RuleCode => "VIP_ADDITIONAL_HALKGUNU";
    public string RuleTitle => "VIP Ek Film Halk Günü Fiyatı";
    public int ExecutionOrder => 5; // After VIP monthly free but before other discounts

    public Task<bool> IsApplicableAsync(PricingContext context, QuoteItemRequest item)
    {
        // This rule applies when:
        // 1. Member is VIP and approved
        // 2. Has already used monthly free ticket
        // 3. Item is for VIP member (not guest)
        return Task.FromResult(
            context.IsVipMember && 
            context.HasUsedMonthlyVipFreeTicket &&
            item.TicketType == TicketType.VIP &&
            !item.IsVipGuest);
    }

    public Task<AppliedRuleDto> CalculateDiscountAsync(PricingContext context, QuoteItemRequest item, decimal basePrice)
    {
        // VIP additional movies get Halk Günü price (50% discount)
        const decimal discountPercentage = 0.50m;
        
        var appliedRule = AppliedRuleDto.CreatePercentageDiscount(
            RuleCode,
            RuleTitle,
            basePrice,
            discountPercentage);

        appliedRule.Details = $"VIP üye ek film - Halk günü fiyatı (%50 indirim)";

        return Task.FromResult(appliedRule);
    }
}
