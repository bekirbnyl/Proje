using Sinema.Domain.Enums;

namespace Sinema.Application.DTOs.Tickets;

/// <summary>
/// Response for getting ticket details
/// </summary>
public class GetTicketResponse
{
    /// <summary>
    /// Unique identifier of the ticket
    /// </summary>
    public Guid TicketId { get; set; }

    /// <summary>
    /// Unique ticket code for identification
    /// </summary>
    public string TicketCode { get; set; } = string.Empty;

    /// <summary>
    /// ID of the screening this ticket is for
    /// </summary>
    public Guid ScreeningId { get; set; }

    /// <summary>
    /// ID of the seat this ticket is for
    /// </summary>
    public Guid SeatId { get; set; }

    /// <summary>
    /// Type of ticket
    /// </summary>
    public TicketType TicketType { get; set; }

    /// <summary>
    /// Channel through which the ticket was sold
    /// </summary>
    public TicketChannel Channel { get; set; }

    /// <summary>
    /// Final price paid for this ticket
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// When the ticket was sold
    /// </summary>
    public DateTime SoldAt { get; set; }

    /// <summary>
    /// Applied pricing details in JSON format
    /// </summary>
    public string? AppliedPricingJson { get; set; }

    /// <summary>
    /// Seat information
    /// </summary>
    public SeatInfo Seat { get; set; } = new();

    /// <summary>
    /// Screening information
    /// </summary>
    public ScreeningInfo Screening { get; set; } = new();
}

/// <summary>
/// Seat information for ticket response
/// </summary>
public class SeatInfo
{
    public int Row { get; set; }
    public int Col { get; set; }
    public string Label { get; set; } = string.Empty;
}

/// <summary>
/// Screening information for ticket response
/// </summary>
public class ScreeningInfo
{
    public DateTime StartAt { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string HallName { get; set; } = string.Empty;
}
