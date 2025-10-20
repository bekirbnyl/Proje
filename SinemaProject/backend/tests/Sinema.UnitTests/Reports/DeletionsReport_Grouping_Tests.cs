using Microsoft.EntityFrameworkCore;
using Sinema.Application.DTOs.Reports.Deletions;
using Sinema.Domain.Entities;
using Sinema.Infrastructure.Persistence;
using Sinema.Infrastructure.Services.Reporting;

namespace Sinema.UnitTests.Reports;

/// <summary>
/// Unit tests for deletions report grouping logic
/// </summary>
public class DeletionsReport_Grouping_Tests : IDisposable
{
    private readonly SinemaDbContext _context;
    private readonly DeletionsReportBuilder _builder;

    public DeletionsReport_Grouping_Tests()
    {
        var options = new DbContextOptionsBuilder<SinemaDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new SinemaDbContext(options);
        _builder = new DeletionsReportBuilder(_context);
    }

    [Fact]
    public async Task BuildReportAsync_GroupByEntity_GroupsCorrectly()
    {
        // Arrange
        await SeedTestData();

        var request = new DeletionsReportRequest
        {
            From = DateTime.Today.AddDays(-7),
            To = DateTime.Today,
            By = "entity"
        };

        // Act
        var result = await _builder.BuildReportAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Data.Count > 0);
        Assert.All(result.Data, row => Assert.NotNull(row.EntityName));
    }

    [Fact]
    public async Task BuildReportAsync_GroupByReason_GroupsCorrectly()
    {
        // Arrange
        await SeedTestData();

        var request = new DeletionsReportRequest
        {
            From = DateTime.Today.AddDays(-7),
            To = DateTime.Today,
            By = "reason"
        };

        // Act
        var result = await _builder.BuildReportAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.All(result.Data, row => Assert.NotNull(row.Reason));
    }

    [Fact]
    public async Task BuildReportAsync_WithDeletionData_CalculatesCorrectCounts()
    {
        // Arrange
        await SeedTestData();

        var request = new DeletionsReportRequest
        {
            From = DateTime.Today.AddDays(-7),
            To = DateTime.Today,
            By = "entity"
        };

        // Act
        var result = await _builder.BuildReportAsync(request);

        // Assert
        Assert.True(result.Summary.TotalDeletions > 0);
        Assert.True(result.Summary.UniqueEntities > 0);
    }

    private async Task SeedTestData()
    {
        var deletionLog1 = DeletionLog.Create("Movie", Guid.NewGuid(), "Duplicate entry", Guid.NewGuid());
        var deletionLog2 = DeletionLog.Create("Screening", Guid.NewGuid(), "Cancelled", Guid.NewGuid());
        var deletionLog3 = DeletionLog.Create("Movie", Guid.NewGuid(), "Outdated content", Guid.NewGuid());

        deletionLog1.DeletedAt = DateTime.Today.AddDays(-2);
        deletionLog2.DeletedAt = DateTime.Today.AddDays(-1);
        deletionLog3.DeletedAt = DateTime.Today.AddDays(-3);

        _context.DeletionLogs.AddRange(deletionLog1, deletionLog2, deletionLog3);
        await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
