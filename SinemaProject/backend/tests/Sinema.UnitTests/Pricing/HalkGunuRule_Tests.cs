using Sinema.Application.DTOs.Pricing;
using Sinema.Application.Policies.Pricing;
using Sinema.Application.Policies.Pricing.Rules;
using Sinema.Domain.Entities;
using Sinema.Domain.Enums;

namespace Sinema.UnitTests.Pricing;

public class HalkGunuRule_Tests
{
    private readonly HalkGunuRule _rule;

    public HalkGunuRule_Tests()
    {
        _rule = new HalkGunuRule();
    }

    [Fact]
    public void RuleProperties_ShouldHaveCorrectValues()
    {
        // Assert
        Assert.Equal("HALK_GUNU_50", _rule.RuleCode);
        Assert.Equal("Halk Günü %50 İndirim", _rule.RuleTitle);
        Assert.Equal(10, _rule.ExecutionOrder);
    }

    [Fact]
    public async Task IsApplicableAsync_WithSpecialDayFlag_ShouldReturnTrue()
    {
        // Arrange
        var screening = new Screening { IsSpecialDay = true };
        var context = CreatePricingContext(screening, halkGunuSetting: "Wednesday");
        var item = new QuoteItemRequest { TicketType = TicketType.Full };

        // Act
        var result = await _rule.IsApplicableAsync(context, item);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsApplicableAsync_WithMatchingHalkGunuSetting_ShouldReturnTrue()
    {
        // Arrange
        var screening = new Screening { IsSpecialDay = false };
        var wednesday = DateTime.Parse("2025-09-17"); // A Wednesday
        var context = CreatePricingContext(screening, halkGunuSetting: "Wednesday", now: wednesday);
        var item = new QuoteItemRequest { TicketType = TicketType.Full };

        // Act
        var result = await _rule.IsApplicableAsync(context, item);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsApplicableAsync_WithNonMatchingDay_ShouldReturnFalse()
    {
        // Arrange
        var screening = new Screening { IsSpecialDay = false };
        var tuesday = DateTime.Parse("2025-09-16"); // A Tuesday
        var context = CreatePricingContext(screening, halkGunuSetting: "Wednesday", now: tuesday);
        var item = new QuoteItemRequest { TicketType = TicketType.Full };

        // Act
        var result = await _rule.IsApplicableAsync(context, item);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CalculateDiscountAsync_ShouldReturn50PercentDiscount()
    {
        // Arrange
        var screening = new Screening { IsSpecialDay = true };
        var context = CreatePricingContext(screening);
        var item = new QuoteItemRequest { TicketType = TicketType.Full };
        var basePrice = 100m;

        // Act
        var result = await _rule.CalculateDiscountAsync(context, item, basePrice);

        // Assert
        Assert.Equal("HALK_GUNU_50", result.Code);
        Assert.Equal("Halk Günü %50 İndirim", result.Title);
        Assert.Equal(50m, result.AmountOff);
        Assert.Equal(50m, result.FinalPrice);
    }

    [Fact]
    public async Task CalculateDiscountAsync_WithSpecialDayFlag_ShouldHaveCorrectDetails()
    {
        // Arrange
        var screening = new Screening { IsSpecialDay = true };
        var context = CreatePricingContext(screening);
        var item = new QuoteItemRequest { TicketType = TicketType.Full };
        var basePrice = 100m;

        // Act
        var result = await _rule.CalculateDiscountAsync(context, item, basePrice);

        // Assert
        Assert.Contains("Özel gün olarak işaretlenmiş", result.Details);
    }

    [Fact]
    public async Task CalculateDiscountAsync_WithHalkGunuSetting_ShouldHaveCorrectDetails()
    {
        // Arrange
        var screening = new Screening { IsSpecialDay = false };
        var wednesday = DateTime.Parse("2025-09-17");
        var context = CreatePricingContext(screening, halkGunuSetting: "Wednesday", now: wednesday);
        var item = new QuoteItemRequest { TicketType = TicketType.Full };
        var basePrice = 100m;

        // Act
        var result = await _rule.CalculateDiscountAsync(context, item, basePrice);

        // Assert
        Assert.Contains("Halk Günü (Wednesday)", result.Details);
    }

    private static PricingContext CreatePricingContext(
        Screening screening, 
        string? halkGunuSetting = null, 
        DateTime? now = null)
    {
        return PricingContext.Create(
            screening,
            member: null,
            basePrice: 100m,
            now: now ?? DateTime.UtcNow,
            halkGunuSetting: halkGunuSetting);
    }
}
