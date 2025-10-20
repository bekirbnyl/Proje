namespace Sinema.Infrastructure.Services.Configuration;

/// <summary>
/// Configuration options for reports
/// </summary>
public class ReportsOptions
{
    /// <summary>
    /// Root directory for storing generated reports
    /// </summary>
    public string StorageRoot { get; set; } = "storage/reports";

    /// <summary>
    /// Default timezone for report generation
    /// </summary>
    public string DefaultTimezone { get; set; } = "UTC";
}
