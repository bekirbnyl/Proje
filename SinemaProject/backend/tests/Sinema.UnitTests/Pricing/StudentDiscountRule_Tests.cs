using Sinema.Application.DTOs.Pricing;
using Sinema.Application.Policies.Pricing;
using Sinema.Application.Policies.Pricing.Rules;
using Sinema.Domain.Entities;
using Sinema.Domain.Enums;

namespace Sinema.UnitTests.Pricing;

public class StudentDiscountRule_Tests
{
    private readonly StudentDiscountRule _rule;

    public StudentDiscountRule_Tests()
    {
        _rule = new StudentDiscountRule();
    }

    [Fact]
    public void RuleProperties_ShouldHaveCorrectValues()
    {
        // Assert
        Assert.Equal("STUDENT_40", _rule.RuleCode);
        Assert.Equal("Öğrenci %40 İndirim", _rule.RuleTitle);
        Assert.Equal(20, _rule.ExecutionOrder);
    }

    [Fact]
    public async Task IsApplicableAsync_WithStudentTicket_ShouldReturnTrue()
    {
        // Arrange
        var screening = new Screening();
        var context = CreatePricingContext(screening);
        var item = new QuoteItemRequest { TicketType = TicketType.Student };

        // Act
        var result = await _rule.IsApplicableAsync(context, item);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData(TicketType.Full)]
    [InlineData(TicketType.VIP)]
    [InlineData(TicketType.VIPGuest)]
    public async Task IsApplicableAsync_WithNonStudentTickets_ShouldReturnFalse(TicketType ticketType)
    {
        // Arrange
        var screening = new Screening();
        var context = CreatePricingContext(screening);
        var item = new QuoteItemRequest { TicketType = ticketType };

        // Act
        var result = await _rule.IsApplicableAsync(context, item);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CalculateDiscountAsync_ShouldReturn40PercentDiscount()
    {
        // Arrange
        var screening = new Screening();
        var context = CreatePricingContext(screening);
        var item = new QuoteItemRequest { TicketType = TicketType.Student };
        var basePrice = 100m;

        // Act
        var result = await _rule.CalculateDiscountAsync(context, item, basePrice);

        // Assert
        Assert.Equal("STUDENT_40", result.Code);
        Assert.Equal("Öğrenci %40 İndirim", result.Title);
        Assert.Equal(40m, result.AmountOff);
        Assert.Equal(60m, result.FinalPrice);
        Assert.Contains("Öğrenci bileti", result.Details);
    }

    [Theory]
    [InlineData(100, 40, 60)]
    [InlineData(80, 32, 48)]
    [InlineData(150, 60, 90)]
    [InlineData(75, 30, 45)]
    public async Task CalculateDiscountAsync_WithDifferentPrices_ShouldCalculateCorrectDiscount(
        decimal basePrice, decimal expectedDiscount, decimal expectedFinalPrice)
    {
        // Arrange
        var screening = new Screening();
        var context = CreatePricingContext(screening);
        var item = new QuoteItemRequest { TicketType = TicketType.Student };

        // Act
        var result = await _rule.CalculateDiscountAsync(context, item, basePrice);

        // Assert
        Assert.Equal(expectedDiscount, result.AmountOff);
        Assert.Equal(expectedFinalPrice, result.FinalPrice);
    }

    [Fact]
    public async Task IsApplicableAsync_WithMemberOrAnonymous_ShouldNotMatter()
    {
        // Arrange
        var screening = new Screening();
        var member = new Member { VipStatus = true };
        
        // Test with member
        var contextWithMember = CreatePricingContext(screening, member);
        var item = new QuoteItemRequest { TicketType = TicketType.Student };

        // Act & Assert
        var resultWithMember = await _rule.IsApplicableAsync(contextWithMember, item);
        Assert.True(resultWithMember);

        // Test without member
        var contextWithoutMember = CreatePricingContext(screening);
        var resultWithoutMember = await _rule.IsApplicableAsync(contextWithoutMember, item);
        Assert.True(resultWithoutMember);
    }

    private static PricingContext CreatePricingContext(Screening screening, Member? member = null)
    {
        return PricingContext.Create(
            screening,
            member,
            basePrice: 100m,
            DateTime.UtcNow);
    }
}
