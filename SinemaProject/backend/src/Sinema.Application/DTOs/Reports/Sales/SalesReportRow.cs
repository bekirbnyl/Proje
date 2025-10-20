namespace Sinema.Application.DTOs.Reports.Sales;

/// <summary>
/// Represents a single row of sales report data
/// </summary>
public class SalesReportRow
{
    /// <summary>
    /// Date of the sales data
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Name of the film (if grouped by film)
    /// </summary>
    public string? FilmName { get; set; }

    /// <summary>
    /// Name of the hall (if grouped by hall)
    /// </summary>
    public string? HallName { get; set; }

    /// <summary>
    /// Screening identifier (if grouped by screening)
    /// </summary>
    public string? ScreeningId { get; set; }

    /// <summary>
    /// Sales channel
    /// </summary>
    public string Channel { get; set; } = string.Empty;

    /// <summary>
    /// Number of tickets sold
    /// </summary>
    public int TicketCount { get; set; }

    /// <summary>
    /// Gross revenue (base price total)
    /// </summary>
    public decimal Gross { get; set; }

    /// <summary>
    /// Total discount amount (difference between gross and net)
    /// </summary>
    public decimal DiscountTotal { get; set; }

    /// <summary>
    /// Net revenue (final price total)
    /// </summary>
    public decimal Net { get; set; }

    /// <summary>
    /// Average price per ticket
    /// </summary>
    public decimal AvgPrice { get; set; }

    /// <summary>
    /// Occupancy rate (sold seats / total seats)
    /// </summary>
    public decimal Occupancy { get; set; }

    /// <summary>
    /// Total number of seats available for this screening
    /// </summary>
    public int TotalSeats { get; set; }

    /// <summary>
    /// Number of seats sold
    /// </summary>
    public int SoldSeats { get; set; }

    /// <summary>
    /// Calculates derived metrics
    /// </summary>
    public void CalculateMetrics()
    {
        AvgPrice = TicketCount > 0 ? Net / TicketCount : 0;
        Occupancy = TotalSeats > 0 ? (decimal)SoldSeats / TotalSeats : 0;
        DiscountTotal = Gross - Net;
    }
}
