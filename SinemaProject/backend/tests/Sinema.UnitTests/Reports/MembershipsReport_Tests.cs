using Microsoft.EntityFrameworkCore;
using Sinema.Application.DTOs.Reports.Memberships;
using Sinema.Domain.Entities;
using Sinema.Infrastructure.Persistence;
using Sinema.Infrastructure.Services.Reporting;

namespace Sinema.UnitTests.Reports;

/// <summary>
/// Unit tests for memberships report logic
/// </summary>
public class MembershipsReport_Tests : IDisposable
{
    private readonly SinemaDbContext _context;
    private readonly MembershipsReportBuilder _builder;

    public MembershipsReport_Tests()
    {
        var options = new DbContextOptionsBuilder<SinemaDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new SinemaDbContext(options);
        _builder = new MembershipsReportBuilder(_context);
    }

    [Fact]
    public async Task BuildReportAsync_WithMembershipData_CalculatesCorrectMetrics()
    {
        // Arrange
        await SeedTestData();

        var request = new MembershipsReportRequest
        {
            From = DateTime.Today.AddDays(-7),
            To = DateTime.Today,
            Grain = "daily"
        };

        // Act
        var result = await _builder.BuildReportAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Summary.TotalApplications > 0);
    }

    [Fact]
    public async Task BuildReportAsync_GroupByDaily_GroupsCorrectly()
    {
        // Arrange
        await SeedTestData();

        var request = new MembershipsReportRequest
        {
            From = DateTime.Today.AddDays(-7),
            To = DateTime.Today,
            Grain = "daily"
        };

        // Act
        var result = await _builder.BuildReportAsync(request);

        // Assert
        Assert.All(result.Data, row => Assert.True(row.Date >= request.From && row.Date <= request.To));
    }

    private async Task SeedTestData()
    {
        var member1 = new Member 
        { 
            Id = Guid.NewGuid(), 
            FullName = "Test User 1", 
            Email = "test1@example.com", 
            VipStatus = false,
            CreatedAt = DateTime.Today.AddDays(-2)
        };

        var member2 = new Member 
        { 
            Id = Guid.NewGuid(), 
            FullName = "Test User 2", 
            Email = "test2@example.com", 
            VipStatus = true,
            CreatedAt = DateTime.Today.AddDays(-1)
        };

        var approval = MemberApproval.CreateApproval(member2.Id, "Approved", Guid.NewGuid());
        approval.CreatedAt = DateTime.Today.AddDays(-1);

        _context.Members.AddRange(member1, member2);
        _context.MemberApprovals.Add(approval);
        await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
