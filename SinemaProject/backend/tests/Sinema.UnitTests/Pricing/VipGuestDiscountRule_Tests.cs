using Sinema.Application.DTOs.Pricing;
using Sinema.Application.Policies.Pricing;
using Sinema.Application.Policies.Pricing.Rules;
using Sinema.Domain.Entities;
using Sinema.Domain.Enums;

namespace Sinema.UnitTests.Pricing;

public class VipGuestDiscountRule_Tests
{
    private readonly VipGuestDiscountRule _rule;

    public VipGuestDiscountRule_Tests()
    {
        _rule = new VipGuestDiscountRule();
    }

    [Fact]
    public void RuleProperties_ShouldHaveCorrectValues()
    {
        // Assert
        Assert.Equal("VIP_GUEST_20", _rule.RuleCode);
        Assert.Equal("VIP Misafir %20 İndirim", _rule.RuleTitle);
        Assert.Equal(21, _rule.ExecutionOrder);
    }

    [Fact]
    public async Task IsApplicableAsync_WithVipMemberAndVipGuestFlag_ShouldReturnTrue()
    {
        // Arrange
        var screening = new Screening();
        var member = CreateVipMember();
        var context = CreatePricingContext(screening, member, currentVipGuestIndex: 0);
        var item = new QuoteItemRequest { TicketType = TicketType.Full, IsVipGuest = true };

        // Act
        var result = await _rule.IsApplicableAsync(context, item);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsApplicableAsync_WithVipMemberAndVipGuestTicketType_ShouldReturnTrue()
    {
        // Arrange
        var screening = new Screening();
        var member = CreateVipMember();
        var context = CreatePricingContext(screening, member, currentVipGuestIndex: 0);
        var item = new QuoteItemRequest { TicketType = TicketType.VIPGuest };

        // Act
        var result = await _rule.IsApplicableAsync(context, item);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsApplicableAsync_WithNonVipMember_ShouldReturnFalse()
    {
        // Arrange
        var screening = new Screening();
        var member = new Member { VipStatus = false, Approvals = new[] { new MemberApproval { Approved = true } } };
        var context = CreatePricingContext(screening, member, currentVipGuestIndex: 0);
        var item = new QuoteItemRequest { TicketType = TicketType.Full, IsVipGuest = true };

        // Act
        var result = await _rule.IsApplicableAsync(context, item);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsApplicableAsync_WithUnapprovedVipMember_ShouldReturnFalse()
    {
        // Arrange
        var screening = new Screening();
        var member = new Member { VipStatus = true, Approvals = new[] { new MemberApproval { Approved = false } } };
        var context = CreatePricingContext(screening, member, currentVipGuestIndex: 0);
        var item = new QuoteItemRequest { TicketType = TicketType.Full, IsVipGuest = true };

        // Act
        var result = await _rule.IsApplicableAsync(context, item);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsApplicableAsync_WithNonVipGuestItem_ShouldReturnFalse()
    {
        // Arrange
        var screening = new Screening();
        var member = CreateVipMember();
        var context = CreatePricingContext(screening, member, currentVipGuestIndex: 0);
        var item = new QuoteItemRequest { TicketType = TicketType.Full, IsVipGuest = false };

        // Act
        var result = await _rule.IsApplicableAsync(context, item);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData(0, true)]  // First VIP guest
    [InlineData(1, true)]  // Second VIP guest
    [InlineData(2, false)] // Third VIP guest (should not get discount)
    [InlineData(3, false)] // Fourth VIP guest (should not get discount)
    public async Task IsApplicableAsync_WithVipGuestIndexLimits_ShouldRespectMaxLimit(
        int currentVipGuestIndex, bool expectedResult)
    {
        // Arrange
        var screening = new Screening();
        var member = CreateVipMember();
        var context = CreatePricingContext(screening, member, currentVipGuestIndex: currentVipGuestIndex);
        var item = new QuoteItemRequest { TicketType = TicketType.Full, IsVipGuest = true };

        // Act
        var result = await _rule.IsApplicableAsync(context, item);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public async Task IsApplicableAsync_WithNoMember_ShouldReturnFalse()
    {
        // Arrange
        var screening = new Screening();
        var context = CreatePricingContext(screening, member: null, currentVipGuestIndex: 0);
        var item = new QuoteItemRequest { TicketType = TicketType.Full, IsVipGuest = true };

        // Act
        var result = await _rule.IsApplicableAsync(context, item);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CalculateDiscountAsync_ShouldReturn20PercentDiscount()
    {
        // Arrange
        var screening = new Screening();
        var member = CreateVipMember();
        var context = CreatePricingContext(screening, member, currentVipGuestIndex: 0);
        var item = new QuoteItemRequest { TicketType = TicketType.Full, IsVipGuest = true };
        var basePrice = 100m;

        // Act
        var result = await _rule.CalculateDiscountAsync(context, item, basePrice);

        // Assert
        Assert.Equal("VIP_GUEST_20", result.Code);
        Assert.Equal("VIP Misafir %20 İndirim", result.Title);
        Assert.Equal(20m, result.AmountOff);
        Assert.Equal(80m, result.FinalPrice);
        Assert.Contains("VIP misafir indirimi", result.Details);
        Assert.Contains("(1/2)", result.Details); // First of max 2
    }

    [Theory]
    [InlineData(100, 20, 80)]
    [InlineData(150, 30, 120)]
    [InlineData(80, 16, 64)]
    [InlineData(75, 15, 60)]
    public async Task CalculateDiscountAsync_WithDifferentPrices_ShouldCalculateCorrectDiscount(
        decimal basePrice, decimal expectedDiscount, decimal expectedFinalPrice)
    {
        // Arrange
        var screening = new Screening();
        var member = CreateVipMember();
        var context = CreatePricingContext(screening, member, currentVipGuestIndex: 0);
        var item = new QuoteItemRequest { TicketType = TicketType.Full, IsVipGuest = true };

        // Act
        var result = await _rule.CalculateDiscountAsync(context, item, basePrice);

        // Assert
        Assert.Equal(expectedDiscount, result.AmountOff);
        Assert.Equal(expectedFinalPrice, result.FinalPrice);
    }

    [Theory]
    [InlineData(0, "(1/2)")]
    [InlineData(1, "(2/2)")]
    public async Task CalculateDiscountAsync_WithDifferentGuestIndexes_ShouldShowCorrectPosition(
        int currentVipGuestIndex, string expectedPosition)
    {
        // Arrange
        var screening = new Screening();
        var member = CreateVipMember();
        var context = CreatePricingContext(screening, member, currentVipGuestIndex: currentVipGuestIndex);
        var item = new QuoteItemRequest { TicketType = TicketType.Full, IsVipGuest = true };
        var basePrice = 100m;

        // Act
        var result = await _rule.CalculateDiscountAsync(context, item, basePrice);

        // Assert
        Assert.Contains(expectedPosition, result.Details);
    }

    private static Member CreateVipMember()
    {
        return new Member
        {
            VipStatus = true,
            Approvals = new[] { new MemberApproval { Approved = true } }
        };
    }

    private static PricingContext CreatePricingContext(
        Screening screening, 
        Member? member, 
        int currentVipGuestIndex)
    {
        var context = PricingContext.Create(
            screening,
            member,
            basePrice: 100m,
            DateTime.UtcNow);
        
        context.CurrentVipGuestIndex = currentVipGuestIndex;
        return context;
    }
}
