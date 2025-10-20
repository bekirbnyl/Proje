using Sinema.Application.DTOs.Pricing;

namespace Sinema.Application.Abstractions.Pricing;

/// <summary>
/// Main pricing engine responsible for calculating quotes by applying pricing rules
/// </summary>
public interface IPricingEngine
{
    /// <summary>
    /// Calculates a price quote for the given request
    /// Applies all applicable pricing rules and returns the final prices with breakdown
    /// </summary>
    /// <param name="request">The pricing request containing screening, member, and items</param>
    /// <returns>Complete price quote with item-level breakdown</returns>
    Task<PriceQuoteResponse> CalculateQuoteAsync(PriceQuoteRequest request);

    /// <summary>
    /// Gets the base price for a ticket
    /// Priority: Screening.Price -> Settings("BaseTicketPrice") -> Default 100.00 TRY
    /// </summary>
    /// <param name="screeningId">ID of the screening</param>
    /// <returns>Base ticket price</returns>
    Task<decimal> GetBasePriceAsync(Guid screeningId);
}
