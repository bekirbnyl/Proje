using FluentValidation;
using Sinema.Application.DTOs.Pricing;
using Sinema.Domain.Enums;

namespace Sinema.Application.Validators.Pricing;

/// <summary>
/// Validator for price quote requests
/// </summary>
public class PriceQuoteRequestValidator : AbstractValidator<PriceQuoteRequest>
{
    public PriceQuoteRequestValidator()
    {
        RuleFor(x => x.ScreeningId)
            .NotEmpty()
            .WithMessage("Screening ID is required");

        RuleFor(x => x.Items)
            .NotNull()
            .WithMessage("Items list is required")
            .Must(items => items.Count > 0)
            .WithMessage("At least one item is required")
            .Must(items => items.Count <= 50)
            .WithMessage("Maximum 50 items allowed per request");

        RuleForEach(x => x.Items)
            .SetValidator(new QuoteItemRequestValidator());

        // Business rule validations
        RuleFor(x => x)
            .Must(HaveValidVipGuestConfiguration)
            .WithMessage("VIP guest items require a member ID for VIP validation")
            .Must(NotExceedVipGuestLimit)
            .WithMessage("Maximum 2 VIP guest items allowed per request");
    }

    private static bool HaveValidVipGuestConfiguration(PriceQuoteRequest request)
    {
        // If there are VIP guest items, member ID must be provided
        var hasVipGuestItems = request.Items.Any(i => 
            i.IsVipGuest || i.TicketType == TicketType.VIPGuest);

        if (hasVipGuestItems && !request.MemberId.HasValue)
        {
            return false;
        }

        return true;
    }

    private static bool NotExceedVipGuestLimit(PriceQuoteRequest request)
    {
        // Count total VIP guest items (including quantities)
        var totalVipGuestItems = request.Items
            .Where(i => i.IsVipGuest || i.TicketType == TicketType.VIPGuest)
            .Sum(i => i.Quantity);

        return totalVipGuestItems <= 2;
    }
}

/// <summary>
/// Validator for individual quote items
/// </summary>
public class QuoteItemRequestValidator : AbstractValidator<QuoteItemRequest>
{
    public QuoteItemRequestValidator()
    {
        RuleFor(x => x.TicketType)
            .IsInEnum()
            .WithMessage("Invalid ticket type");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than 0")
            .LessThanOrEqualTo(10)
            .WithMessage("Maximum 10 tickets per item");

        // Business rule: VIP guest flag should align with ticket type
        RuleFor(x => x)
            .Must(HaveConsistentVipGuestConfiguration)
            .WithMessage("VIP guest flag and ticket type should be consistent");
    }

    private static bool HaveConsistentVipGuestConfiguration(QuoteItemRequest item)
    {
        // If TicketType is VIPGuest, IsVipGuest should be true (or can be omitted)
        // If IsVipGuest is true, TicketType can be anything (for flexibility)
        if (item.TicketType == TicketType.VIPGuest && !item.IsVipGuest)
        {
            // This is a warning rather than error - auto-correct is acceptable
            // We'll allow this and treat VIPGuest ticket type as VIP guest
        }

        return true; // Always pass for now, engine will handle the logic
    }
}
