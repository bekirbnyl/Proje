using Microsoft.EntityFrameworkCore;
using Sinema.Application.Abstractions.Reporting;
using Sinema.Application.DTOs.Reports.Sales;
using Sinema.Domain.Enums;
using Sinema.Infrastructure.Persistence;

namespace Sinema.Infrastructure.Services.Reporting;

/// <summary>
/// Builder for generating sales reports
/// </summary>
public class SalesReportBuilder : IReportBuilder<SalesReportRequest, SalesReportResponse>
{
    private readonly SinemaDbContext _context;

    /// <summary>
    /// Initializes a new instance of the SalesReportBuilder
    /// </summary>
    /// <param name="context">Database context</param>
    public SalesReportBuilder(SinemaDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Builds a sales report based on the provided request parameters
    /// </summary>
    /// <param name="request">The request containing report parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The sales report response containing aggregated data</returns>
    public async Task<SalesReportResponse> BuildReportAsync(SalesReportRequest request, CancellationToken cancellationToken = default)
    {
        var response = new SalesReportResponse
        {
            From = request.From,
            To = request.To,
            Grain = request.Grain,
            GroupBy = request.By,
            Channel = request.Channel
        };

        // Build the base query
        var query = _context.Tickets
            .AsNoTracking()
            .Include(t => t.Screening)
                .ThenInclude(s => s.Movie)
            .Include(t => t.Screening)
                .ThenInclude(s => s.Hall)
            .Include(t => t.Screening)
                .ThenInclude(s => s.SeatLayout)
                .ThenInclude(sl => sl.Seats)
            .Where(t => t.SoldAt >= request.From && t.SoldAt <= request.To.AddDays(1).AddTicks(-1));

        // Apply channel filter
        if (!request.NormalizedChannel.Equals("all"))
        {
            var channelFilter = GetChannelFromString(request.NormalizedChannel);
            if (channelFilter.HasValue)
            {
                query = query.Where(t => t.Channel == channelFilter.Value);
            }
        }

        // Execute query and group in memory for better performance
        var tickets = await query.ToListAsync(cancellationToken);

        // Group by the specified criteria
        var groupedData = request.NormalizedBy switch
        {
            "film" => GroupByFilm(tickets, request),
            "hall" => GroupByHall(tickets, request),
            "screening" => GroupByScreening(tickets, request),
            _ => throw new ArgumentException($"Invalid groupBy parameter: {request.By}")
        };

        response.Data = groupedData.ToList();

        // Calculate summary
        response.Summary = CalculateSummary(response.Data);

        return response;
    }

    /// <summary>
    /// Groups tickets by film
    /// </summary>
    private IEnumerable<SalesReportRow> GroupByFilm(List<Domain.Entities.Ticket> tickets, SalesReportRequest request)
    {
        return tickets
            .GroupBy(t => new
            {
                Date = GetDateGrouping(t.SoldAt, request.NormalizedGrain),
                FilmName = t.Screening.Movie.Title,
                Channel = GetChannelString(t.Channel)
            })
            .Select(g => CreateSalesReportRow(g, g.Key.Date, g.Key.FilmName, null, null, g.Key.Channel));
    }

    /// <summary>
    /// Groups tickets by hall
    /// </summary>
    private IEnumerable<SalesReportRow> GroupByHall(List<Domain.Entities.Ticket> tickets, SalesReportRequest request)
    {
        return tickets
            .GroupBy(t => new
            {
                Date = GetDateGrouping(t.SoldAt, request.NormalizedGrain),
                HallName = t.Screening.Hall.Name,
                Channel = GetChannelString(t.Channel)
            })
            .Select(g => CreateSalesReportRow(g, g.Key.Date, null, g.Key.HallName, null, g.Key.Channel));
    }

    /// <summary>
    /// Groups tickets by screening
    /// </summary>
    private IEnumerable<SalesReportRow> GroupByScreening(List<Domain.Entities.Ticket> tickets, SalesReportRequest request)
    {
        return tickets
            .GroupBy(t => new
            {
                Date = GetDateGrouping(t.SoldAt, request.NormalizedGrain),
                ScreeningId = t.ScreeningId.ToString(),
                Channel = GetChannelString(t.Channel)
            })
            .Select(g => CreateSalesReportRow(g, g.Key.Date, null, null, g.Key.ScreeningId, g.Key.Channel));
    }

    /// <summary>
    /// Creates a sales report row from grouped tickets
    /// </summary>
    private SalesReportRow CreateSalesReportRow(
        IGrouping<dynamic, Domain.Entities.Ticket> group,
        DateTime date,
        string? filmName,
        string? hallName,
        string? screeningId,
        string channel)
    {
        var tickets = group.ToList();
        var firstTicket = tickets.First();
        var seatLayout = firstTicket.Screening.SeatLayout;
        var totalSeats = seatLayout.Seats.Count;

        // Calculate gross price (base price before discounts)
        var grossTotal = tickets.Sum(t => GetBasePrice(t));
        var netTotal = tickets.Sum(t => t.Price);

        var row = new SalesReportRow
        {
            Date = date,
            FilmName = filmName,
            HallName = hallName,
            ScreeningId = screeningId,
            Channel = channel,
            TicketCount = tickets.Count,
            Gross = grossTotal,
            Net = netTotal,
            TotalSeats = totalSeats,
            SoldSeats = tickets.Count
        };

        row.CalculateMetrics();
        return row;
    }

    /// <summary>
    /// Gets the base price of a ticket (before discounts)
    /// </summary>
    private decimal GetBasePrice(Domain.Entities.Ticket ticket)
    {
        // If we have pricing information in JSON, parse it
        if (!string.IsNullOrEmpty(ticket.AppliedPricingJson))
        {
            try
            {
                // Try to extract base price from JSON
                // For now, return the final price as we don't have base price stored separately
                return ticket.Price;
            }
            catch
            {
                // Fallback to final price
                return ticket.Price;
            }
        }

        // Calculate approximate base price using discount percentages
        var discountPercentage = ticket.GetDiscountPercentage();
        if (discountPercentage > 0)
        {
            return ticket.Price / (1 - discountPercentage);
        }

        return ticket.Price;
    }

    /// <summary>
    /// Groups date by the specified grain
    /// </summary>
    private DateTime GetDateGrouping(DateTime date, string grain)
    {
        return grain switch
        {
            "daily" => date.Date,
            "weekly" => date.Date.AddDays(-(int)date.DayOfWeek), // Start of week (Sunday)
            "monthly" => new DateTime(date.Year, date.Month, 1),
            _ => date.Date
        };
    }

    /// <summary>
    /// Converts ticket channel to string representation
    /// </summary>
    private string GetChannelString(TicketChannel channel)
    {
        return channel switch
        {
            TicketChannel.Online => "web",
            TicketChannel.BoxOffice => "boxoffice",
            TicketChannel.Mobile => "mobile",
            _ => "unknown"
        };
    }

    /// <summary>
    /// Converts string to ticket channel enum
    /// </summary>
    private TicketChannel? GetChannelFromString(string channel)
    {
        return channel switch
        {
            "web" => TicketChannel.Online,
            "boxoffice" => TicketChannel.BoxOffice,
            "mobile" => TicketChannel.Mobile,
            _ => null
        };
    }

    /// <summary>
    /// Calculates summary statistics for the report
    /// </summary>
    private SalesReportSummary CalculateSummary(IList<SalesReportRow> data)
    {
        if (!data.Any())
        {
            return new SalesReportSummary();
        }

        return new SalesReportSummary
        {
            TotalTickets = data.Sum(d => d.TicketCount),
            TotalGross = data.Sum(d => d.Gross),
            TotalDiscount = data.Sum(d => d.DiscountTotal),
            TotalNet = data.Sum(d => d.Net),
            TotalSeatsAvailable = data.Sum(d => d.TotalSeats),
            TotalSeatsSold = data.Sum(d => d.SoldSeats),
            AvgPrice = data.Sum(d => d.TicketCount) > 0 ? data.Sum(d => d.Net) / data.Sum(d => d.TicketCount) : 0,
            OverallOccupancy = data.Sum(d => d.TotalSeats) > 0 ? (decimal)data.Sum(d => d.SoldSeats) / data.Sum(d => d.TotalSeats) : 0
        };
    }
}
