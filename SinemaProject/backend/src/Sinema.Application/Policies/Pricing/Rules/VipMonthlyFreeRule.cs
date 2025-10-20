using Sinema.Application.Abstractions.Pricing;
using Sinema.Application.DTOs.Pricing;

namespace Sinema.Application.Policies.Pricing.Rules;

/// <summary>
/// Pricing rule for VIP monthly free ticket
/// Applies to approved VIP members who haven't used their monthly free ticket
/// This rule overrides all other discounts when applied
/// </summary>
public class VipMonthlyFreeRule : IPricingRule
{
    public string RuleCode => "VIP_MONTHLY_FREE";
    public string RuleTitle => "VIP Aylık Ücretsiz Bilet";
    public int ExecutionOrder => 1; // Highest priority - executes first

    public Task<bool> IsApplicableAsync(PricingContext context, QuoteItemRequest item)
    {
        // VIP monthly free applies only to:
        // 1. VIP members who are approved
        // 2. Haven't used their monthly free ticket yet
        // 3. Not specifically a VIP guest item
        return Task.FromResult(
            context.IsVipMember && 
            !context.HasUsedMonthlyVipFreeTicket &&
            !item.IsVipGuest);
    }

    public Task<AppliedRuleDto> CalculateDiscountAsync(PricingContext context, QuoteItemRequest item, decimal basePrice)
    {
        // Full discount - ticket is free
        var appliedRule = AppliedRuleDto.CreateFreeTicket(basePrice);
        
        appliedRule.Details = $"VIP üyelik aylık ücretsiz bilet hakkı (Bu ay kullanılan: {context.VipFreeTicketsUsedThisMonth})";

        return Task.FromResult(appliedRule);
    }
}
