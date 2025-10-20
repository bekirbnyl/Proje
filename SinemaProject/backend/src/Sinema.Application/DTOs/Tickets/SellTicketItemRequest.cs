using Sinema.Domain.Enums;

namespace Sinema.Application.DTOs.Tickets;

/// <summary>
/// Represents a single ticket item in a sell request (for direct box office sales)
/// </summary>
public class SellTicketItemRequest
{
    /// <summary>
    /// ID of the seat to purchase
    /// </summary>
    public Guid SeatId { get; set; }

    /// <summary>
    /// Type of ticket (Full, Student, VIP, VIPGuest)
    /// </summary>
    public TicketType TicketType { get; set; }

    /// <summary>
    /// Whether this is for a VIP guest (used with VIPGuest ticket type)
    /// </summary>
    public bool IsVipGuest { get; set; }
}
