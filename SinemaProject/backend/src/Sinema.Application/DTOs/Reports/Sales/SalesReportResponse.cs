namespace Sinema.Application.DTOs.Reports.Sales;

/// <summary>
/// Response DTO for sales report containing aggregated data
/// </summary>
public class SalesReportResponse
{
    /// <summary>
    /// Start date of the report range
    /// </summary>
    public DateTime From { get; set; }

    /// <summary>
    /// End date of the report range
    /// </summary>
    public DateTime To { get; set; }

    /// <summary>
    /// Granularity used for grouping
    /// </summary>
    public string Grain { get; set; } = string.Empty;

    /// <summary>
    /// Grouping criteria used
    /// </summary>
    public string GroupBy { get; set; } = string.Empty;

    /// <summary>
    /// Channel filter applied
    /// </summary>
    public string Channel { get; set; } = string.Empty;

    /// <summary>
    /// Collection of sales data rows
    /// </summary>
    public IList<SalesReportRow> Data { get; set; } = new List<SalesReportRow>();

    /// <summary>
    /// Summary statistics for the entire report period
    /// </summary>
    public SalesReportSummary Summary { get; set; } = new();

    /// <summary>
    /// When the report was generated
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Total number of data rows
    /// </summary>
    public int TotalRows => Data.Count;
}

/// <summary>
/// Summary statistics for the sales report
/// </summary>
public class SalesReportSummary
{
    /// <summary>
    /// Total tickets sold in the period
    /// </summary>
    public int TotalTickets { get; set; }

    /// <summary>
    /// Total gross revenue
    /// </summary>
    public decimal TotalGross { get; set; }

    /// <summary>
    /// Total discount amount
    /// </summary>
    public decimal TotalDiscount { get; set; }

    /// <summary>
    /// Total net revenue
    /// </summary>
    public decimal TotalNet { get; set; }

    /// <summary>
    /// Average price per ticket
    /// </summary>
    public decimal AvgPrice { get; set; }

    /// <summary>
    /// Overall occupancy rate
    /// </summary>
    public decimal OverallOccupancy { get; set; }

    /// <summary>
    /// Total seats available in the period
    /// </summary>
    public int TotalSeatsAvailable { get; set; }

    /// <summary>
    /// Total seats sold in the period
    /// </summary>
    public int TotalSeatsSold { get; set; }
}
