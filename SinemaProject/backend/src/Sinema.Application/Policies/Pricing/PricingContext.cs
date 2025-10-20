using Sinema.Domain.Entities;

namespace Sinema.Application.Policies.Pricing;

/// <summary>
/// Context information for pricing rule evaluation
/// Contains all necessary data for pricing rules to make decisions
/// </summary>
public class PricingContext
{
    /// <summary>
    /// The screening being priced
    /// </summary>
    public Screening Screening { get; set; } = null!;

    /// <summary>
    /// The member making the request (null for anonymous requests)
    /// </summary>
    public Member? Member { get; set; }

    /// <summary>
    /// Base price for tickets in this screening
    /// </summary>
    public decimal BasePrice { get; set; }

    /// <summary>
    /// Current date/time for rule evaluation
    /// </summary>
    public DateTime Now { get; set; }

    /// <summary>
    /// Halk Günü setting value (e.g., "Wednesday")
    /// </summary>
    public string? HalkGunuSetting { get; set; }

    /// <summary>
    /// Number of VIP free tickets the member has used this month
    /// Only relevant if Member is not null and VIP
    /// </summary>
    public int VipFreeTicketsUsedThisMonth { get; set; }

    /// <summary>
    /// Total number of VIP guest items being processed in this request
    /// Used to limit VIP guest discount to max 2 items
    /// </summary>
    public int TotalVipGuestItemsInRequest { get; set; }

    /// <summary>
    /// Current VIP guest item index being processed (0-based)
    /// Used to apply discount only to first 2 VIP guest items
    /// </summary>
    public int CurrentVipGuestIndex { get; set; }

    /// <summary>
    /// Checks if the member is VIP and approved
    /// </summary>
    public bool IsVipMember => Member?.VipStatus == true && Member.IsApproved;

    /// <summary>
    /// Checks if today is Halk Günü based on the setting
    /// </summary>
    public bool IsHalkGunu
    {
        get
        {
            // First check if screening has IsSpecialDay flag
            if (Screening.IsSpecialDay)
                return true;

            // If no setting, return false
            if (string.IsNullOrEmpty(HalkGunuSetting))
                return false;

            // Check if today matches the Halk Günü setting
            return Now.DayOfWeek.ToString().Equals(HalkGunuSetting, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Checks if this is the first weekday show
    /// </summary>
    public bool IsFirstWeekdayShow => Screening.IsFirstShowWeekday;

    /// <summary>
    /// Checks if the member has already used their monthly VIP free ticket
    /// </summary>
    public bool HasUsedMonthlyVipFreeTicket => VipFreeTicketsUsedThisMonth > 0;

    /// <summary>
    /// Creates a pricing context for the given screening and member
    /// </summary>
    /// <param name="screening">The screening</param>
    /// <param name="member">The member (optional)</param>
    /// <param name="basePrice">Base ticket price</param>
    /// <param name="now">Current date/time</param>
    /// <param name="halkGunuSetting">Halk Günü setting</param>
    /// <param name="vipFreeTicketsUsed">VIP free tickets used this month</param>
    /// <returns>Pricing context</returns>
    public static PricingContext Create(
        Screening screening,
        Member? member,
        decimal basePrice,
        DateTime now,
        string? halkGunuSetting = null,
        int vipFreeTicketsUsed = 0)
    {
        return new PricingContext
        {
            Screening = screening,
            Member = member,
            BasePrice = basePrice,
            Now = now,
            HalkGunuSetting = halkGunuSetting,
            VipFreeTicketsUsedThisMonth = vipFreeTicketsUsed
        };
    }
}
