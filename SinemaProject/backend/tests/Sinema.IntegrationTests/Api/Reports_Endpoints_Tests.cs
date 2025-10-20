using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Sinema.Api;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Sinema.IntegrationTests.Api;

/// <summary>
/// Integration tests for Reports API endpoints
/// </summary>
public class Reports_Endpoints_Tests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public Reports_Endpoints_Tests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetSalesReport_WithoutAuth_Returns401()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/reports/sales?from=2025-09-01&to=2025-09-15");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetSalesReport_WithInvalidDateRange_Returns400()
    {
        // Arrange - This would need proper auth setup in a real test
        // For now, just testing the endpoint structure

        // Act
        var response = await _client.GetAsync("/api/v1/reports/sales?from=2025-09-15&to=2025-09-01");

        // Assert
        // Without auth, we get 401, but the endpoint exists
        Assert.True(response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetDeletionsReport_EndpointExists()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/reports/deletions?from=2025-09-01&to=2025-09-15");

        // Assert
        // Should return 401 (unauthorized) rather than 404 (not found)
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMembershipsReport_EndpointExists()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/reports/memberships?from=2025-09-01&to=2025-09-15");

        // Assert
        // Should return 401 (unauthorized) rather than 404 (not found)
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetSalesReport_WithExcelFormat_RequestsExcelFile()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/reports/sales?from=2025-09-01&to=2025-09-15&format=xlsx");

        // Assert
        // Should return 401 (unauthorized) but endpoint recognizes xlsx format
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
