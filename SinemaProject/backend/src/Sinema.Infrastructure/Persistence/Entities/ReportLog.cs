namespace Sinema.Infrastructure.Persistence.Entities;

/// <summary>
/// Represents a log entry for a report generation execution
/// </summary>
public class ReportLog
{
    /// <summary>
    /// Unique identifier for the log entry
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key to the report job
    /// </summary>
    public Guid JobId { get; set; }

    /// <summary>
    /// When the report generation started
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// When the report generation finished (null if still running)
    /// </summary>
    public DateTime? FinishedAt { get; set; }

    /// <summary>
    /// Status of the report generation (Running, Completed, Failed)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Additional message or error details
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Path to the generated output file (if successful)
    /// </summary>
    public string? OutputFilePath { get; set; }

    /// <summary>
    /// Size of the generated file in bytes
    /// </summary>
    public long? FileSizeBytes { get; set; }

    /// <summary>
    /// Number of rows in the generated report
    /// </summary>
    public int? RowCount { get; set; }

    /// <summary>
    /// Navigation property to the report job
    /// </summary>
    public virtual ReportJob Job { get; set; } = null!;

    /// <summary>
    /// Execution status enumeration
    /// </summary>
    public static class ExecutionStatus
    {
        public const string Running = "Running";
        public const string Completed = "Completed";
        public const string Failed = "Failed";
        public const string Cancelled = "Cancelled";
    }

    /// <summary>
    /// Duration of the report generation
    /// </summary>
    public TimeSpan? Duration => FinishedAt?.Subtract(StartedAt);

    /// <summary>
    /// Creates a new report log entry
    /// </summary>
    /// <param name="jobId">The job ID</param>
    /// <param name="status">Initial status</param>
    /// <returns>New ReportLog instance</returns>
    public static ReportLog Create(Guid jobId, string status = ExecutionStatus.Running)
    {
        return new ReportLog
        {
            Id = Guid.NewGuid(),
            JobId = jobId,
            StartedAt = DateTime.UtcNow,
            Status = status
        };
    }

    /// <summary>
    /// Marks the log entry as completed
    /// </summary>
    /// <param name="outputFilePath">Path to the generated file</param>
    /// <param name="fileSizeBytes">Size of the generated file</param>
    /// <param name="rowCount">Number of rows in the report</param>
    /// <param name="message">Optional completion message</param>
    public void Complete(string? outputFilePath = null, long? fileSizeBytes = null, int? rowCount = null, string? message = null)
    {
        FinishedAt = DateTime.UtcNow;
        Status = ExecutionStatus.Completed;
        OutputFilePath = outputFilePath;
        FileSizeBytes = fileSizeBytes;
        RowCount = rowCount;
        Message = message;
    }

    /// <summary>
    /// Marks the log entry as failed
    /// </summary>
    /// <param name="errorMessage">Error message</param>
    public void Fail(string errorMessage)
    {
        FinishedAt = DateTime.UtcNow;
        Status = ExecutionStatus.Failed;
        Message = errorMessage;
    }
}
