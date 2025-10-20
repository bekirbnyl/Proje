using Sinema.Application.DTOs.Pricing;
using Sinema.Application.Policies.Pricing;
using Sinema.Application.Policies.Pricing.Rules;
using Sinema.Domain.Entities;
using Sinema.Domain.Enums;

namespace Sinema.UnitTests.Pricing;

public class VipMonthlyFreeRule_Tests
{
    private readonly VipMonthlyFreeRule _rule;

    public VipMonthlyFreeRule_Tests()
    {
        _rule = new VipMonthlyFreeRule();
    }

    [Fact]
    public void RuleProperties_ShouldHaveCorrectValues()
    {
        // Assert
        Assert.Equal("VIP_MONTHLY_FREE", _rule.RuleCode);
        Assert.Equal("VIP Aylık Ücretsiz Bilet", _rule.RuleTitle);
        Assert.Equal(1, _rule.ExecutionOrder); // Highest priority
    }

    [Fact]
    public async Task IsApplicableAsync_WithVipMemberNoUsage_ShouldReturnTrue()
    {
        // Arrange
        var screening = new Screening();
        var member = CreateVipMember();
        var context = CreatePricingContext(screening, member, vipFreeTicketsUsed: 0);
        var item = new QuoteItemRequest { TicketType = TicketType.Full };

        // Act
        var result = await _rule.IsApplicableAsync(context, item);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsApplicableAsync_WithVipMemberAlreadyUsed_ShouldReturnFalse()
    {
        // Arrange
        var screening = new Screening();
        var member = CreateVipMember();
        var context = CreatePricingContext(screening, member, vipFreeTicketsUsed: 1);
        var item = new QuoteItemRequest { TicketType = TicketType.Full };

        // Act
        var result = await _rule.IsApplicableAsync(context, item);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsApplicableAsync_WithNonVipMember_ShouldReturnFalse()
    {
        // Arrange
        var screening = new Screening();
        var member = new Member { VipStatus = false, Approvals = new[] { new MemberApproval { Approved = true } } };
        var context = CreatePricingContext(screening, member, vipFreeTicketsUsed: 0);
        var item = new QuoteItemRequest { TicketType = TicketType.Full };

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
        var context = CreatePricingContext(screening, member, vipFreeTicketsUsed: 0);
        var item = new QuoteItemRequest { TicketType = TicketType.Full };

        // Act
        var result = await _rule.IsApplicableAsync(context, item);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsApplicableAsync_WithVipGuestItem_ShouldReturnFalse()
    {
        // Arrange
        var screening = new Screening();
        var member = CreateVipMember();
        var context = CreatePricingContext(screening, member, vipFreeTicketsUsed: 0);
        var item = new QuoteItemRequest { TicketType = TicketType.Full, IsVipGuest = true };

        // Act
        var result = await _rule.IsApplicableAsync(context, item);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsApplicableAsync_WithNoMember_ShouldReturnFalse()
    {
        // Arrange
        var screening = new Screening();
        var context = CreatePricingContext(screening, member: null);
        var item = new QuoteItemRequest { TicketType = TicketType.Full };

        // Act
        var result = await _rule.IsApplicableAsync(context, item);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CalculateDiscountAsync_ShouldReturnFreeTicket()
    {
        // Arrange
        var screening = new Screening();
        var member = CreateVipMember();
        var context = CreatePricingContext(screening, member, vipFreeTicketsUsed: 0);
        var item = new QuoteItemRequest { TicketType = TicketType.Full };
        var basePrice = 150m;

        // Act
        var result = await _rule.CalculateDiscountAsync(context, item, basePrice);

        // Assert
        Assert.Equal("VIP_MONTHLY_FREE", result.Code);
        Assert.Equal("VIP Aylık Ücretsiz Bilet", result.Title);
        Assert.Equal(150m, result.AmountOff); // Full price off
        Assert.Equal(0m, result.FinalPrice); // Free
        Assert.Contains("VIP üyelik aylık ücretsiz bilet hakkı", result.Details);
        Assert.Contains("Bu ay kullanılan: 0", result.Details);
    }

    [Theory]
    [InlineData(100)]
    [InlineData(80)]
    [InlineData(200)]
    [InlineData(75.50)]
    public async Task CalculateDiscountAsync_WithDifferentPrices_ShouldAlwaysReturnFree(decimal basePrice)
    {
        // Arrange
        var screening = new Screening();
        var member = CreateVipMember();
        var context = CreatePricingContext(screening, member, vipFreeTicketsUsed: 0);
        var item = new QuoteItemRequest { TicketType = TicketType.Full };

        // Act
        var result = await _rule.CalculateDiscountAsync(context, item, basePrice);

        // Assert
        Assert.Equal(basePrice, result.AmountOff);
        Assert.Equal(0m, result.FinalPrice);
    }

    [Fact]
    public async Task CalculateDiscountAsync_ShouldIncludeUsageInDetails()
    {
        // Arrange
        var screening = new Screening();
        var member = CreateVipMember();
        var context = CreatePricingContext(screening, member, vipFreeTicketsUsed: 0);
        var item = new QuoteItemRequest { TicketType = TicketType.Full };
        var basePrice = 100m;

        // Act
        var result = await _rule.CalculateDiscountAsync(context, item, basePrice);

        // Assert
        Assert.Contains("Bu ay kullanılan: 0", result.Details);
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
        int vipFreeTicketsUsed = 0)
    {
        return PricingContext.Create(
            screening,
            member,
            basePrice: 100m,
            DateTime.UtcNow,
            vipFreeTicketsUsed: vipFreeTicketsUsed);
    }
}
