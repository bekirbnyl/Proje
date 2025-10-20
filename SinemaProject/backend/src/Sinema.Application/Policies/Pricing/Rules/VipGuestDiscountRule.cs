using Sinema.Application.Abstractions.Pricing;
using Sinema.Application.DTOs.Pricing;
using Sinema.Domain.Enums;

namespace Sinema.Application.Policies.Pricing.Rules;

/// <summary>
/// Pricing rule for VIP guest discount - 20% discount
/// Applies to VIP guest items (max 2 per request) when member is VIP
/// </summary>
public class VipGuestDiscountRule : IPricingRule
{
    public string RuleCode => "VIP_GUEST_20";
    public string RuleTitle => "VIP Misafir %20 İndirim";
    public int ExecutionOrder => 21; // After student discount, same level

    private const int MaxVipGuestDiscounts = 2; // Max 2 VIP guest discounts per request

    public Task<bool> IsApplicableAsync(PricingContext context, QuoteItemRequest item)
    {
        // VIP guest discount applies when:
        // 1. Member is VIP and approved
        // 2. Item is marked as VIP guest OR ticket type is VIPGuest
        // 3. Haven't exceeded max VIP guest discounts (2) in this request
        var isVipGuestItem = item.IsVipGuest || item.TicketType == TicketType.VIPGuest;
        var withinLimit = context.CurrentVipGuestIndex < MaxVipGuestDiscounts;
        
        return Task.FromResult(
            context.IsVipMember && 
            isVipGuestItem && 
            withinLimit);
    }

    public Task<AppliedRuleDto> CalculateDiscountAsync(PricingContext context, QuoteItemRequest item, decimal basePrice)
    {
        // 20% discount for VIP guests
        const decimal discountPercentage = 0.20m;
        
        var appliedRule = AppliedRuleDto.CreatePercentageDiscount(
            RuleCode,
            RuleTitle,
            basePrice,
            discountPercentage);

        appliedRule.Details = $"VIP misafir indirimi - %20 indirim ({context.CurrentVipGuestIndex + 1}/{MaxVipGuestDiscounts})";

        return Task.FromResult(appliedRule);
    }
}
