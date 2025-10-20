namespace Sinema.Application.Abstractions.Pricing;

/// <summary>
/// Service for tracking VIP member benefit usage
/// </summary>
public interface IVipUsageService
{
    /// <summary>
    /// Gets the number of free tickets the VIP member has used this month
    /// Counts tickets with Price = 0 where member is VIP and ticket was sold this month
    /// </summary>
    /// <param name="memberId">ID of the VIP member</param>
    /// <returns>Number of free tickets used this month</returns>
    Task<int> GetVipFreeTicketCountThisMonthAsync(Guid memberId);

    /// <summary>
    /// Checks if the VIP member has used their monthly free ticket
    /// </summary>
    /// <param name="memberId">ID of the VIP member</param>
    /// <returns>True if the member has already used their free ticket this month</returns>
    Task<bool> HasUsedMonthlyFreeTicketAsync(Guid memberId);
}
