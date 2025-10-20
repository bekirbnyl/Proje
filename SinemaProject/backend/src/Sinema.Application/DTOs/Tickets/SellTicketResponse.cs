using Sinema.Domain.Enums;

namespace Sinema.Application.DTOs.Tickets;

/// <summary>
/// Response for a ticket sale transaction
/// </summary>
public class SellTicketResponse
{
    /// <summary>
    /// Status of the payment transaction
    /// </summary>
    public PaymentStatus PaymentStatus { get; set; }

    /// <summary>
    /// Total price before any discounts were applied
    /// </summary>
    public decimal TotalBefore { get; set; }

    /// <summary>
    /// Total price after all discounts and rules were applied
    /// </summary>
    public decimal TotalAfter { get; set; }

    /// <summary>
    /// List of individual ticket items that were created
    /// </summary>
    public List<TicketItemResponse> Items { get; set; } = new();

    /// <summary>
    /// External payment transaction reference (if applicable)
    /// </summary>
    public string? PaymentReference { get; set; }
}
