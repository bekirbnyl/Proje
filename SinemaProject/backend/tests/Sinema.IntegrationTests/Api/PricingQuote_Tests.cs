using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Sinema.Application.DTOs.Pricing;
using Sinema.Domain.Entities;
using Sinema.Domain.Enums;
using Sinema.Infrastructure.Persistence;
using System.Net.Http.Json;
using System.Text.Json;

namespace Sinema.IntegrationTests.Api;

public class PricingQuote_Tests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public PricingQuote_Tests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
        });
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetQuote_WithBasicRequest_ShouldReturnSuccessfulResponse()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SinemaDbContext>();
        
        var (screening, _) = await SeedTestDataAsync(context);

        var request = new PriceQuoteRequest
        {
            ScreeningId = screening.Id,
            Items = new List<QuoteItemRequest>
            {
                new() { TicketType = TicketType.Full, Quantity = 1 }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/pricing/quote", request);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var quote = JsonSerializer.Deserialize<PriceQuoteResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(quote);
        Assert.Equal("TRY", quote.Currency);
        Assert.Single(quote.Items);
        Assert.True(quote.TotalBefore > 0);
        Assert.True(quote.TotalAfter > 0);
    }

    [Fact]
    public async Task GetQuote_WithHalkGunuScreening_ShouldApply50PercentDiscount()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SinemaDbContext>();
        
        var (screening, _) = await SeedTestDataAsync(context);
        
        // Mark screening as special day
        screening.IsSpecialDay = true;
        await context.SaveChangesAsync();

        var request = new PriceQuoteRequest
        {
            ScreeningId = screening.Id,
            Items = new List<QuoteItemRequest>
            {
                new() { TicketType = TicketType.Full, Quantity = 1 }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/pricing/quote", request);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var quote = JsonSerializer.Deserialize<PriceQuoteResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(quote);
        var item = quote.Items.First();
        Assert.Contains("HALK_GUNU", item.AppliedRule.Code);
        Assert.Equal(item.BasePrice * 0.5m, item.FinalPrice);
    }

    [Fact]
    public async Task GetQuote_WithFirstWeekdayShow_ShouldApply50PercentDiscount()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SinemaDbContext>();
        
        var (screening, _) = await SeedTestDataAsync(context);
        
        // Mark screening as first weekday show
        screening.IsFirstShowWeekday = true;
        await context.SaveChangesAsync();

        var request = new PriceQuoteRequest
        {
            ScreeningId = screening.Id,
            Items = new List<QuoteItemRequest>
            {
                new() { TicketType = TicketType.Full, Quantity = 1 }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/pricing/quote", request);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var quote = JsonSerializer.Deserialize<PriceQuoteResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(quote);
        var item = quote.Items.First();
        Assert.Contains("FIRST_SHOW", item.AppliedRule.Code);
        Assert.Equal(item.BasePrice * 0.5m, item.FinalPrice);
    }

    [Fact]
    public async Task GetQuote_WithStudentTicket_ShouldApply40PercentDiscount()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SinemaDbContext>();
        
        var (screening, _) = await SeedTestDataAsync(context);

        var request = new PriceQuoteRequest
        {
            ScreeningId = screening.Id,
            Items = new List<QuoteItemRequest>
            {
                new() { TicketType = TicketType.Student, Quantity = 1 }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/pricing/quote", request);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var quote = JsonSerializer.Deserialize<PriceQuoteResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(quote);
        var item = quote.Items.First();
        Assert.Contains("STUDENT", item.AppliedRule.Code);
        Assert.Equal(item.BasePrice * 0.6m, item.FinalPrice); // 40% off = 60% remaining
    }

    [Fact]
    public async Task GetQuote_WithVipMemberFirstFreeTicket_ShouldBeCompletelyFree()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SinemaDbContext>();
        
        var (screening, member) = await SeedTestDataAsync(context);
        
        // Make member VIP and approved
        member.VipStatus = true;
        member.Approvals.Add(new MemberApproval { Approved = true, CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var request = new PriceQuoteRequest
        {
            ScreeningId = screening.Id,
            MemberId = member.Id,
            Items = new List<QuoteItemRequest>
            {
                new() { TicketType = TicketType.Full, Quantity = 1 }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/pricing/quote", request);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var quote = JsonSerializer.Deserialize<PriceQuoteResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(quote);
        var item = quote.Items.First();
        Assert.Contains("VIP_MONTHLY_FREE", item.AppliedRule.Code);
        Assert.Equal(0m, item.FinalPrice);
        Assert.Equal(0m, quote.TotalAfter);
    }

    [Fact]
    public async Task GetQuote_WithMultipleItems_ShouldApplyCorrectRulesToEach()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SinemaDbContext>();
        
        var (screening, member) = await SeedTestDataAsync(context);
        
        // Make member VIP and approved
        member.VipStatus = true;
        member.Approvals.Add(new MemberApproval { Approved = true, CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var request = new PriceQuoteRequest
        {
            ScreeningId = screening.Id,
            MemberId = member.Id,
            Items = new List<QuoteItemRequest>
            {
                new() { TicketType = TicketType.Full, Quantity = 1 }, // Should be VIP free
                new() { TicketType = TicketType.Student, Quantity = 1 }, // Should be student discount
                new() { TicketType = TicketType.Full, IsVipGuest = true, Quantity = 1 } // Should be VIP guest discount
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/pricing/quote", request);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var quote = JsonSerializer.Deserialize<PriceQuoteResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(quote);
        Assert.Equal(3, quote.Items.Count);

        // First item should be VIP free
        var vipFreeItem = quote.Items.First();
        Assert.Contains("VIP_MONTHLY_FREE", vipFreeItem.AppliedRule.Code);
        Assert.Equal(0m, vipFreeItem.FinalPrice);

        // Second item should be student discount
        var studentItem = quote.Items.Skip(1).First();
        Assert.Contains("STUDENT", studentItem.AppliedRule.Code);
        
        // Third item should be VIP guest discount
        var vipGuestItem = quote.Items.Skip(2).First();
        Assert.Contains("VIP_GUEST", vipGuestItem.AppliedRule.Code);
    }

    [Fact]
    public async Task GetQuote_WithInvalidScreeningId_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new PriceQuoteRequest
        {
            ScreeningId = Guid.NewGuid(), // Non-existent screening
            Items = new List<QuoteItemRequest>
            {
                new() { TicketType = TicketType.Full, Quantity = 1 }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/pricing/quote", request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetQuote_WithEmptyItems_ShouldReturnValidationError()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SinemaDbContext>();
        
        var (screening, _) = await SeedTestDataAsync(context);

        var request = new PriceQuoteRequest
        {
            ScreeningId = screening.Id,
            Items = new List<QuoteItemRequest>() // Empty items
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/pricing/quote", request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetBasePrice_WithValidScreening_ShouldReturnPrice()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SinemaDbContext>();
        
        var (screening, _) = await SeedTestDataAsync(context);

        // Act
        var response = await _client.GetAsync($"/api/v1/pricing/base-price/{screening.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var price = JsonSerializer.Deserialize<decimal>(content);
        
        Assert.True(price > 0);
    }

    [Fact]
    public async Task GetQuickQuote_WithValidParameters_ShouldReturnQuoteItem()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SinemaDbContext>();
        
        var (screening, _) = await SeedTestDataAsync(context);

        // Act
        var response = await _client.GetAsync($"/api/v1/pricing/quick-quote/{screening.Id}?ticketType=Student");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var item = JsonSerializer.Deserialize<QuoteItemResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(item);
        Assert.True(item.BasePrice > 0);
        Assert.True(item.FinalPrice > 0);
        Assert.NotNull(item.AppliedRule);
    }

    private static async Task<(Screening screening, Member member)> SeedTestDataAsync(SinemaDbContext context)
    {
        // Create a test movie
        var movie = new Movie
        {
            Id = Guid.NewGuid(),
            Title = "Test Movie",
            Description = "Test Description",
            Duration = 120,
            Genre = "Action",
            Director = "Test Director",
            Cast = "Test Cast",
            ReleaseDate = DateTime.UtcNow.AddDays(-30),
            CreatedAt = DateTime.UtcNow
        };

        // Create a test hall
        var hall = new Hall
        {
            Id = Guid.NewGuid(),
            Name = "Test Hall",
            Capacity = 100,
            CreatedAt = DateTime.UtcNow
        };

        // Create a test seat layout
        var seatLayout = new SeatLayout
        {
            Id = Guid.NewGuid(),
            HallId = hall.Id,
            Name = "Test Layout",
            RowCount = 10,
            SeatsPerRow = 10,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            Version = 1
        };

        // Create a test screening
        var screening = new Screening
        {
            Id = Guid.NewGuid(),
            MovieId = movie.Id,
            HallId = hall.Id,
            SeatLayoutId = seatLayout.Id,
            StartAt = DateTime.UtcNow.AddDays(1),
            DurationMinutes = 120,
            IsFirstShowWeekday = false,
            IsSpecialDay = false,
            CreatedAt = DateTime.UtcNow
        };

        // Create a test member
        var member = new Member
        {
            Id = Guid.NewGuid(),
            FullName = "Test Member",
            Email = "test@example.com",
            VipStatus = false,
            CreatedAt = DateTime.UtcNow
        };

        context.Movies.Add(movie);
        context.Halls.Add(hall);
        context.SeatLayouts.Add(seatLayout);
        context.Screenings.Add(screening);
        context.Members.Add(member);

        await context.SaveChangesAsync();

        return (screening, member);
    }
}
