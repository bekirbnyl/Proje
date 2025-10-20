namespace Sinema.Domain.Entities;

/// <summary>
/// Represents a registered member of the cinema system
/// </summary>
public class Member
{
    /// <summary>
    /// Unique identifier for the member
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Full name of the member
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Email address of the member
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Phone number of the member
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Foreign key to the Identity user (nullable for legacy members)
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Whether the member has VIP status
    /// </summary>
    public bool VipStatus { get; set; }

    /// <summary>
    /// VIP membership start date (when VIP status becomes active)
    /// </summary>
    public DateTime? VipStartDate { get; set; }

    /// <summary>
    /// VIP membership end date (when VIP status expires)
    /// </summary>
    public DateTime? VipEndDate { get; set; }

    /// <summary>
    /// Payment ID for VIP membership payment
    /// </summary>
    public Guid? VipPaymentId { get; set; }

    /// <summary>
    /// When the member account was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the member account was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Collection of reservations made by this member
    /// </summary>
    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    /// <summary>
    /// Collection of approval records for this member
    /// </summary>
    public virtual ICollection<MemberApproval> Approvals { get; set; } = new List<MemberApproval>();

    /// <summary>
    /// Collection of credit transactions for this member
    /// </summary>
    public virtual ICollection<MemberCredit> Credits { get; set; } = new List<MemberCredit>();

    /// <summary>
    /// Checks if the member is currently approved
    /// </summary>
    public bool IsApproved => Approvals.Any(a => a.Approved);

    /// <summary>
    /// Checks if the member has active VIP status
    /// </summary>
    public bool IsActiveVip => VipStatus &&
                              VipStartDate.HasValue &&
                              VipStartDate.Value <= DateTime.UtcNow &&
                              VipEndDate.HasValue &&
                              VipEndDate.Value >= DateTime.UtcNow;

    /// <summary>
    /// Gets the latest approval status
    /// </summary>
    public MemberApproval? LatestApproval => Approvals.OrderByDescending(a => a.CreatedAt).FirstOrDefault();

    /// <summary>
    /// Calculates the current credit balance
    /// </summary>
    public decimal GetCreditBalance()
    {
        return Credits.Sum(c => c.Amount);
    }

    /// <summary>
    /// Grants VIP status to the member for a seasonal period
    /// </summary>
    /// <param name="startDate">VIP membership start date</param>
    /// <param name="endDate">VIP membership end date</param>
    /// <param name="paymentId">Payment ID for the VIP membership</param>
    public void GrantVipStatus(DateTime startDate, DateTime endDate, Guid? paymentId = null)
    {
        VipStatus = true;
        VipStartDate = startDate;
        VipEndDate = endDate;
        VipPaymentId = paymentId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Revokes VIP status from the member
    /// </summary>
    public void RevokeVipStatus()
    {
        VipStatus = false;
        VipStartDate = null;
        VipEndDate = null;
        VipPaymentId = null;
        UpdatedAt = DateTime.UtcNow;
    }
}
