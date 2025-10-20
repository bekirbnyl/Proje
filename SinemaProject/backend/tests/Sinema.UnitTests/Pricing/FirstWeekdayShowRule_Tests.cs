using Sinema.Application.DTOs.Pricing;
using Sinema.Application.Policies.Pricing;
using Sinema.Application.Policies.Pricing.Rules;
using Sinema.Domain.Entities;
using Sinema.Domain.Enums;

namespace Sinema.UnitTests.Pricing;

public class FirstWeekdayShowRule_Tests
{
    private readonly FirstWeekdayShowRule _rule;

    public FirstWeekdayShowRule_Tests()
    {
        _rule = new FirstWeekdayShowRule();
    }

    [Fact]
    public void RuleProperties_ShouldHaveCorrectValues()
    {
        // Assert
        Assert.Equal("FIRST_SHOW_50", _rule.RuleCode);
        Assert.Equal("Hafta İçi İlk Seans %50 İndirim", _rule.RuleTitle);
        Assert.Equal(11, _rule.ExecutionOrder);
    }

    [Fact]
    public async Task IsApplicableAsync_WithFirstWeekdayShow_ShouldReturnTrue()
    {
        // Arrange
        var screening = new Screening { IsFirstShowWeekday = true };
        var context = CreatePricingContext(screening);
        var item = new QuoteItemRequest { TicketType = TicketType.Full };

        // Act
        var result = await _rule.IsApplicableAsync(context, item);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsApplicableAsync_WithNonFirstWeekdayShow_ShouldReturnFalse()
    {
        // Arrange
        var screening = new Screening { IsFirstShowWeekday = false };
        var context = CreatePricingContext(screening);
        var item = new QuoteItemRequest { TicketType = TicketType.Full };

        // Act
        var result = await _rule.IsApplicableAsync(context, item);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsApplicableAsync_WithDifferentTicketTypes_ShouldReturnTrue()
    {
        // Arrange
        var screening = new Screening { IsFirstShowWeekday = true };
        var context = CreatePricingContext(screening);

        var ticketTypes = new[] { TicketType.Full, TicketType.Student, TicketType.VIP, TicketType.VIPGuest };

        foreach (var ticketType in ticketTypes)
        {
            var item = new QuoteItemRequest { TicketType = ticketType };

            // Act
            var result = await _rule.IsApplicableAsync(context, item);

            // Assert
            Assert.True(result, $"Rule should apply to {ticketType} tickets");
        }
    }

    [Fact]
    public async Task CalculateDiscountAsync_ShouldReturn50PercentDiscount()
    {
        // Arrange
        var screening = new Screening { IsFirstShowWeekday = true };
        var context = CreatePricingContext(screening);
        var item = new QuoteItemRequest { TicketType = TicketType.Full };
        var basePrice = 120m;

        // Act
        var result = await _rule.CalculateDiscountAsync(context, item, basePrice);

        // Assert
        Assert.Equal("FIRST_SHOW_50", result.Code);
        Assert.Equal("Hafta İçi İlk Seans %50 İndirim", result.Title);
        Assert.Equal(60m, result.AmountOff);
        Assert.Equal(60m, result.FinalPrice);
        Assert.Contains("Hafta içi günün ilk seansı", result.Details);
    }

    [Theory]
    [InlineData(100, 50, 50)]
    [InlineData(80, 40, 40)]
    [InlineData(150, 75, 75)]
    public async Task CalculateDiscountAsync_WithDifferentPrices_ShouldCalculateCorrectDiscount(
        decimal basePrice, decimal expectedDiscount, decimal expectedFinalPrice)
    {
        // Arrange
        var screening = new Screening { IsFirstShowWeekday = true };
        var context = CreatePricingContext(screening);
        var item = new QuoteItemRequest { TicketType = TicketType.Full };

        // Act
        var result = await _rule.CalculateDiscountAsync(context, item, basePrice);

        // Assert
        Assert.Equal(expectedDiscount, result.AmountOff);
        Assert.Equal(expectedFinalPrice, result.FinalPrice);
    }

    private static PricingContext CreatePricingContext(Screening screening)
    {
        return PricingContext.Create(
            screening,
            member: null,
            basePrice: 100m,
            DateTime.UtcNow);
    }
}
