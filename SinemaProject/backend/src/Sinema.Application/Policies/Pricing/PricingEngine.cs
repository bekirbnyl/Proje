using Sinema.Application.Abstractions.Pricing;
using Sinema.Application.Abstractions.Settings;
using Sinema.Application.DTOs.Pricing;
using Sinema.Domain.Interfaces.Repositories;
using Sinema.Domain.Enums;
using Sinema.Domain.Entities;

namespace Sinema.Application.Policies.Pricing;

/// <summary>
/// Main pricing engine that orchestrates pricing rule evaluation
/// Implements the strategy pattern with composition of pricing rules
/// </summary>
public class PricingEngine : IPricingEngine
{
    private readonly IEnumerable<IPricingRule> _pricingRules;
    private readonly IScreeningRepository _screeningRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly IAppSettingsReader _settingsReader;
    private readonly IVipUsageService _vipUsageService;

    public PricingEngine(
        IEnumerable<IPricingRule> pricingRules,
        IScreeningRepository screeningRepository,
        IMemberRepository memberRepository,
        IAppSettingsReader settingsReader,
        IVipUsageService vipUsageService)
    {
        _pricingRules = pricingRules;
        _screeningRepository = screeningRepository;
        _memberRepository = memberRepository;
        _settingsReader = settingsReader;
        _vipUsageService = vipUsageService;
    }

    public async Task<PriceQuoteResponse> CalculateQuoteAsync(PriceQuoteRequest request)
    {
        // Validate basic requirements
        if (request.Items == null || !request.Items.Any())
        {
            throw new ArgumentException("Quote request must contain at least one item", nameof(request));
        }

        // Get screening information
        var screening = await _screeningRepository.GetByIdAsync(request.ScreeningId);
        if (screening == null)
        {
            throw new ArgumentException($"Screening with ID {request.ScreeningId} not found", nameof(request));
        }

        // Get member information if provided
        Member? member = null;
        if (request.MemberId.HasValue)
        {
            member = await _memberRepository.GetByIdAsync(request.MemberId.Value);
            // Note: We don't throw if member not found - just treat as anonymous
        }

        // Get base price
        var basePrice = await GetBasePriceAsync(request.ScreeningId);

        // Create pricing context
        var context = await CreatePricingContextAsync(screening, member, basePrice, request);

        // Process each item
        var responseItems = new List<QuoteItemResponse>();
        var vipGuestIndex = 0;

        foreach (var item in request.Items)
        {
            // Update VIP guest context for this item
            if (item.IsVipGuest || item.TicketType == TicketType.VIPGuest)
            {
                context.CurrentVipGuestIndex = vipGuestIndex;
                vipGuestIndex++;
            }

            // Process multiple quantities as separate items
            for (int i = 0; i < item.Quantity; i++)
            {
                var responseItem = await ProcessSingleItemAsync(context, item, basePrice);
                responseItems.Add(responseItem);

                // If this was a VIP monthly free ticket, update context to reflect usage
                if (responseItem.AppliedRule.Code == "VIP_MONTHLY_FREE")
                {
                    context.VipFreeTicketsUsedThisMonth++;
                }
            }
        }

        // Calculate totals
        var totalBefore = responseItems.Sum(i => i.BasePrice);
        var totalAfter = responseItems.Sum(i => i.FinalPrice);

        return new PriceQuoteResponse
        {
            Currency = "TRY",
            TotalBefore = totalBefore,
            TotalAfter = totalAfter,
            Items = responseItems
        };
    }

    public async Task<decimal> GetBasePriceAsync(Guid screeningId)
    {
        // Priority: Screening.Price -> Settings("BaseTicketPrice") -> Default 100.00 TRY
        
        var screening = await _screeningRepository.GetByIdAsync(screeningId);
        
        // Check if screening has a specific price (assuming there's a Price property)
        // Note: Current Screening entity doesn't have Price property, so this is for future extension
        // if (screening?.Price.HasValue == true)
        // {
        //     return screening.Price.Value;
        // }

        // Try to get from settings
        var settingPrice = await _settingsReader.GetDecimalSettingAsync("BaseTicketPrice");
        if (settingPrice.HasValue)
        {
            return settingPrice.Value;
        }

        // Default fallback
        return 100.00m;
    }

    private async Task<PricingContext> CreatePricingContextAsync(
        Screening screening, 
        Member? member, 
        decimal basePrice, 
        PriceQuoteRequest request)
    {
        // Get Halk Günü setting
        var halkGunuSetting = await _settingsReader.GetHalkGunuAsync();

        // Get VIP usage information if member is VIP
        var vipFreeTicketsUsed = 0;
        if (member?.IsActiveVip == true)
        {
            vipFreeTicketsUsed = await _vipUsageService.GetVipFreeTicketCountThisMonthAsync(member.Id);
        }

        // Count total VIP guest items in request
        var totalVipGuestItems = request.Items.Count(i => 
            i.IsVipGuest || i.TicketType == TicketType.VIPGuest);

        var context = PricingContext.Create(
            screening,
            member,
            basePrice,
            DateTime.UtcNow,
            halkGunuSetting,
            vipFreeTicketsUsed);

        context.TotalVipGuestItemsInRequest = totalVipGuestItems;

        return context;
    }

    private async Task<QuoteItemResponse> ProcessSingleItemAsync(
        PricingContext context, 
        QuoteItemRequest item, 
        decimal basePrice)
    {
        // Get applicable rules sorted by execution order
        var applicableRules = new List<IPricingRule>();
        
        foreach (var rule in _pricingRules.OrderBy(r => r.ExecutionOrder))
        {
            if (await rule.IsApplicableAsync(context, item))
            {
                applicableRules.Add(rule);
            }
        }

        AppliedRuleDto appliedRule;

        if (!applicableRules.Any())
        {
            // No rules applicable - use base price
            appliedRule = AppliedRuleDto.CreateNoDiscount(basePrice);
        }
        else
        {
            // Apply rule composition logic
            appliedRule = await ApplyRuleCompositionAsync(context, item, basePrice, applicableRules);
        }

        return new QuoteItemResponse
        {
            SeatId = item.SeatId,
            BasePrice = basePrice,
            FinalPrice = appliedRule.FinalPrice,
            AppliedRule = appliedRule,
            Quantity = 1 // Individual item always has quantity 1
        };
    }

    private async Task<AppliedRuleDto> ApplyRuleCompositionAsync(
        PricingContext context,
        QuoteItemRequest item,
        decimal basePrice,
        List<IPricingRule> applicableRules)
    {
        // Rule composition logic:
        // 1. VipMonthlyFree overrides all other discounts
        // 2. VipAdditionalMovie (halk günü price) takes precedence for VIP additional movies
        // 3. Other rules don't stack - apply the highest discount

        // Check for VIP monthly free first (highest priority)
        var vipFreeRule = applicableRules.FirstOrDefault(r => r.RuleCode == "VIP_MONTHLY_FREE");
        if (vipFreeRule != null)
        {
            return await vipFreeRule.CalculateDiscountAsync(context, item, basePrice);
        }

        // Check for VIP additional movie rule (second priority)
        var vipAdditionalRule = applicableRules.FirstOrDefault(r => r.RuleCode == "VIP_ADDITIONAL_HALKGUNU");
        if (vipAdditionalRule != null)
        {
            return await vipAdditionalRule.CalculateDiscountAsync(context, item, basePrice);
        }

        // Calculate all other applicable discounts and pick the best one
        var bestRule = AppliedRuleDto.CreateNoDiscount(basePrice);
        decimal bestDiscount = 0;

        foreach (var rule in applicableRules)
        {
            var ruleResult = await rule.CalculateDiscountAsync(context, item, basePrice);
            
            // Compare discount amounts - pick the highest discount
            if (ruleResult.AmountOff > bestDiscount)
            {
                bestDiscount = ruleResult.AmountOff;
                bestRule = ruleResult;
            }
        }

        return bestRule;
    }
}
