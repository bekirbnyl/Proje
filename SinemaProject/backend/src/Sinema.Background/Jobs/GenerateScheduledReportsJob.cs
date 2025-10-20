using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using Sinema.Application.Abstractions.Export;
using Sinema.Application.Abstractions.Reporting;
using Sinema.Application.DTOs.Reports.Deletions;
using Sinema.Application.DTOs.Reports.Memberships;
using Sinema.Application.DTOs.Reports.Sales;
using Sinema.Infrastructure.Persistence;
using Sinema.Infrastructure.Persistence.Entities;
using Sinema.Infrastructure.Services.Configuration;
using System.Text.Json;

namespace Sinema.Background.Jobs;

/// <summary>
/// Background job for generating scheduled reports
/// </summary>
[DisallowConcurrentExecution]
public class GenerateScheduledReportsJob : IJob
{
    private readonly SinemaDbContext _context;
    private readonly IReportBuilder<SalesReportRequest, SalesReportResponse> _salesReportBuilder;
    private readonly IReportBuilder<DeletionsReportRequest, DeletionsReportResponse> _deletionsReportBuilder;
    private readonly IReportBuilder<MembershipsReportRequest, MembershipsReportResponse> _membershipsReportBuilder;
    private readonly IExcelExporter _excelExporter;
    private readonly ILogger<GenerateScheduledReportsJob> _logger;
    private readonly ReportsOptions _reportsOptions;

    /// <summary>
    /// Initializes a new instance of the GenerateScheduledReportsJob
    /// </summary>
    public GenerateScheduledReportsJob(
        SinemaDbContext context,
        IReportBuilder<SalesReportRequest, SalesReportResponse> salesReportBuilder,
        IReportBuilder<DeletionsReportRequest, DeletionsReportResponse> deletionsReportBuilder,
        IReportBuilder<MembershipsReportRequest, MembershipsReportResponse> membershipsReportBuilder,
        IExcelExporter excelExporter,
        ILogger<GenerateScheduledReportsJob> logger,
        IOptions<ReportsOptions> reportsOptions)
    {
        _context = context;
        _salesReportBuilder = salesReportBuilder;
        _deletionsReportBuilder = deletionsReportBuilder;
        _membershipsReportBuilder = membershipsReportBuilder;
        _excelExporter = excelExporter;
        _logger = logger;
        _reportsOptions = reportsOptions.Value;
    }

    /// <summary>
    /// Executes the scheduled report generation job
    /// </summary>
    /// <param name="context">Job execution context</param>
    public async Task Execute(IJobExecutionContext context)
    {
        var cancellationToken = context.CancellationToken;

        _logger.LogInformation("Starting scheduled report generation job at {Timestamp}", DateTime.UtcNow);

        try
        {
            // Get active report jobs
            var activeJobs = await _context.ReportJobs
                .Where(rj => rj.IsActive)
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Found {JobCount} active report jobs", activeJobs.Count);

            foreach (var job in activeJobs)
            {
                await ProcessReportJob(job, cancellationToken);
            }

            _logger.LogInformation("Completed scheduled report generation job at {Timestamp}", DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during scheduled report generation");
            throw;
        }
    }

    /// <summary>
    /// Processes a single report job
    /// </summary>
    private async Task ProcessReportJob(ReportJob job, CancellationToken cancellationToken)
    {
        var reportLog = ReportLog.Create(job.Id);
        _context.ReportLogs.Add(reportLog);
        await _context.SaveChangesAsync(cancellationToken);

        try
        {
            _logger.LogInformation("Processing report job {JobId} of type {JobType}", job.Id, job.GetReportType());

            var outputFilePath = await GenerateReport(job, cancellationToken);
            var fileInfo = new FileInfo(outputFilePath);

            // Get row count from the generated file (approximate)
            var rowCount = await EstimateRowCount(outputFilePath);

            reportLog.Complete(outputFilePath, fileInfo.Length, rowCount, "Report generated successfully");
            job.UpdateLastRun("Completed");

            _logger.LogInformation("Successfully generated report for job {JobId}. File: {FilePath}, Size: {Size} bytes",
                job.Id, outputFilePath, fileInfo.Length);
        }
        catch (Exception ex)
        {
            reportLog.Fail($"Error generating report: {ex.Message}");
            job.UpdateLastRun("Failed");

            _logger.LogError(ex, "Failed to generate report for job {JobId}", job.Id);
        }
        finally
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Generates a report based on the job configuration
    /// </summary>
    private async Task<string> GenerateReport(ReportJob job, CancellationToken cancellationToken)
    {
        var reportType = job.GetReportType();
        var parameters = JsonSerializer.Deserialize<Dictionary<string, object>>(job.ParametersJson);

        // Create date range (default to last 30 days if not specified)
        var to = DateTime.UtcNow.Date;
        var from = to.AddDays(-30);

        if (parameters != null)
        {
            if (parameters.TryGetValue("daysBack", out var daysBackObj) && 
                int.TryParse(daysBackObj.ToString(), out var daysBack))
            {
                from = to.AddDays(-daysBack);
            }
        }

        // Generate filename with timestamp
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss");
        var fileName = $"{reportType.ToString().ToLower()}_{from:yyyy-MM-dd}_{to:yyyy-MM-dd}_{timestamp}";

        // Ensure output directory exists
        var outputDir = Path.Combine(_reportsOptions.StorageRoot, $"{DateTime.UtcNow:yyyy-MM-dd}");
        Directory.CreateDirectory(outputDir);

        var filePath = Path.Combine(outputDir, $"{fileName}.xlsx");

        switch (reportType)
        {
            case ReportJob.ReportType.Sales:
                await GenerateSalesReport(from, to, fileName, filePath, cancellationToken);
                break;
            case ReportJob.ReportType.Deletions:
                await GenerateDeletionsReport(from, to, fileName, filePath, cancellationToken);
                break;
            case ReportJob.ReportType.Memberships:
                await GenerateMembershipsReport(from, to, fileName, filePath, cancellationToken);
                break;
            default:
                throw new ArgumentException($"Unknown report type: {reportType}");
        }

        return filePath;
    }

    /// <summary>
    /// Generates a sales report
    /// </summary>
    private async Task GenerateSalesReport(DateTime from, DateTime to, string fileName, string filePath, CancellationToken cancellationToken)
    {
        var request = new SalesReportRequest
        {
            From = from,
            To = to,
            Grain = "daily",
            By = "film",
            Channel = "all"
        };

        var report = await _salesReportBuilder.BuildReportAsync(request, cancellationToken);
        var (fileBytes, _) = await _excelExporter.ExportToExcelAsync(report.Data, "Sales", fileName, cancellationToken);

        await File.WriteAllBytesAsync(filePath, fileBytes, cancellationToken);
    }

    /// <summary>
    /// Generates a deletions report
    /// </summary>
    private async Task GenerateDeletionsReport(DateTime from, DateTime to, string fileName, string filePath, CancellationToken cancellationToken)
    {
        var request = new DeletionsReportRequest
        {
            From = from,
            To = to,
            By = "entity"
        };

        var report = await _deletionsReportBuilder.BuildReportAsync(request, cancellationToken);
        var (fileBytes, _) = await _excelExporter.ExportToExcelAsync(report.Data, "Deletions", fileName, cancellationToken);

        await File.WriteAllBytesAsync(filePath, fileBytes, cancellationToken);
    }

    /// <summary>
    /// Generates a memberships report
    /// </summary>
    private async Task GenerateMembershipsReport(DateTime from, DateTime to, string fileName, string filePath, CancellationToken cancellationToken)
    {
        var request = new MembershipsReportRequest
        {
            From = from,
            To = to,
            Grain = "daily"
        };

        var report = await _membershipsReportBuilder.BuildReportAsync(request, cancellationToken);
        var (fileBytes, _) = await _excelExporter.ExportToExcelAsync(report.Data, "Memberships", fileName, cancellationToken);

        await File.WriteAllBytesAsync(filePath, fileBytes, cancellationToken);
    }

    /// <summary>
    /// Estimates the number of rows in an Excel file
    /// </summary>
    private async Task<int> EstimateRowCount(string filePath)
    {
        try
        {
            // For Excel files, we can't easily count rows without opening them
            // Return a simple estimate based on file size
            var fileInfo = new FileInfo(filePath);
            return (int)(fileInfo.Length / 100); // Rough estimate: ~100 bytes per row
        }
        catch
        {
            return 0;
        }
    }
}
