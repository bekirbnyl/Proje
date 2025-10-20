using Sinema.Application.Abstractions.Pricing;
using Sinema.Application.DTOs.Pricing;
using Sinema.Domain.Enums;

namespace Sinema.Application.Policies.Pricing.Rules;

/// <summary>
/// Pricing rule for student discount - 40% discount
/// Applies only when TicketType is Student
/// </summary>
public class StudentDiscountRule : IPricingRule
{
    public string RuleCode => "STUDENT_40";
    public string RuleTitle => "Öğrenci %40 İndirim";
    public int ExecutionOrder => 20; // After day-based discounts

    public Task<bool> IsApplicableAsync(PricingContext context, QuoteItemRequest item)
    {
        // Student discount applies only to Student ticket type
        return Task.FromResult(item.TicketType == TicketType.Student);
    }

    public Task<AppliedRuleDto> CalculateDiscountAsync(PricingContext context, QuoteItemRequest item, decimal basePrice)
    {
        // 40% discount for students
        const decimal discountPercentage = 0.40m;
        
        var appliedRule = AppliedRuleDto.CreatePercentageDiscount(
            RuleCode,
            RuleTitle,
            basePrice,
            discountPercentage);

        appliedRule.Details = "Öğrenci bileti - %40 indirim";

        return Task.FromResult(appliedRule);
    }
}
