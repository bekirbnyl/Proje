namespace Sinema.Infrastructure.Persistence.Entities;

/// <summary>
/// Represents a scheduled report generation job
/// </summary>
public class ReportJob
{
    /// <summary>
    /// Unique identifier for the report job
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Type of report (1=Sales, 2=Deletions, 3=Memberships)
    /// </summary>
    public byte Type { get; set; }

    /// <summary>
    /// Cron expression for scheduling the job
    /// </summary>
    public string Cron { get; set; } = string.Empty;

    /// <summary>
    /// Parameters for the report in JSON format
    /// </summary>
    public string ParametersJson { get; set; } = string.Empty;

    /// <summary>
    /// Path where the report output will be stored
    /// </summary>
    public string OutputPath { get; set; } = string.Empty;

    /// <summary>
    /// Whether the job is currently active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// When the job was last executed
    /// </summary>
    public DateTime? LastRunAt { get; set; }

    /// <summary>
    /// Status of the last job execution
    /// </summary>
    public string? LastStatus { get; set; }

    /// <summary>
    /// When the job was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the job was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Collection of log entries for this job
    /// </summary>
    public virtual ICollection<ReportLog> Logs { get; set; } = new List<ReportLog>();

    /// <summary>
    /// Report type enumeration
    /// </summary>
    public enum ReportType : byte
    {
        Sales = 1,
        Deletions = 2,
        Memberships = 3
    }

    /// <summary>
    /// Gets the report type as an enum
    /// </summary>
    public ReportType GetReportType() => (ReportType)Type;

    /// <summary>
    /// Sets the report type from an enum
    /// </summary>
    /// <param name="reportType">The report type</param>
    public void SetReportType(ReportType reportType)
    {
        Type = (byte)reportType;
    }

    /// <summary>
    /// Creates a new report job
    /// </summary>
    /// <param name="type">Report type</param>
    /// <param name="cron">Cron expression</param>
    /// <param name="parametersJson">Parameters in JSON format</param>
    /// <param name="outputPath">Output path</param>
    /// <returns>New ReportJob instance</returns>
    public static ReportJob Create(ReportType type, string cron, string parametersJson, string outputPath)
    {
        return new ReportJob
        {
            Id = Guid.NewGuid(),
            Type = (byte)type,
            Cron = cron,
            ParametersJson = parametersJson,
            OutputPath = outputPath,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Updates the last run status
    /// </summary>
    /// <param name="status">The execution status</param>
    public void UpdateLastRun(string status)
    {
        LastRunAt = DateTime.UtcNow;
        LastStatus = status;
        UpdatedAt = DateTime.UtcNow;
    }
}
