using Sinema.Application.DTOs.Pricing;

namespace Sinema.Application.DTOs.Tickets;

/// <summary>
/// Response for a single ticket item in a sale transaction
/// </summary>
public class TicketItemResponse
{
    /// <summary>
    /// Unique identifier of the created ticket
    /// </summary>
    public Guid TicketId { get; set; }

    /// <summary>
    /// Unique ticket code for identification (e.g., "AB12-34CD")
    /// </summary>
    public string TicketCode { get; set; } = string.Empty;

    /// <summary>
    /// ID of the seat this ticket is for
    /// </summary>
    public Guid SeatId { get; set; }

    /// <summary>
    /// Final price paid for this ticket after all discounts
    /// </summary>
    public decimal FinalPrice { get; set; }

    /// <summary>
    /// Details of the pricing rule that was applied to this ticket
    /// </summary>
    public AppliedRuleDto? AppliedRule { get; set; }
}
