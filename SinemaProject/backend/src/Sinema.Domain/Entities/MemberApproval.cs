namespace Sinema.Domain.Entities;

/// <summary>
/// Represents an approval decision for a member registration
/// </summary>
public class MemberApproval
{
    /// <summary>
    /// Unique identifier for the approval record
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key to the member
    /// </summary>
    public Guid MemberId { get; set; }

    /// <summary>
    /// Whether the member was approved or rejected
    /// </summary>
    public bool Approved { get; set; }

    /// <summary>
    /// Reason for approval or rejection
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// ID of the user who made the approval decision
    /// </summary>
    public Guid? ApprovedBy { get; set; }

    /// <summary>
    /// When the approval decision was made
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Navigation property to the member
    /// </summary>
    public virtual Member Member { get; set; } = null!;

    /// <summary>
    /// Gets the approval status as a human-readable string
    /// </summary>
    public string Status => Approved ? "Approved" : "Rejected";

    /// <summary>
    /// Creates an approval record
    /// </summary>
    /// <param name="memberId">The member ID</param>
    /// <param name="reason">Reason for approval</param>
    /// <param name="approvedBy">Who approved it</param>
    /// <returns>MemberApproval instance</returns>
    public static MemberApproval CreateApproval(Guid memberId, string reason, Guid? approvedBy = null)
    {
        return new MemberApproval
        {
            Id = Guid.NewGuid(),
            MemberId = memberId,
            Approved = true,
            Reason = reason,
            ApprovedBy = approvedBy,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a pending approval record
    /// </summary>
    /// <param name="memberId">The member ID</param>
    /// <param name="reason">Reason for pending approval</param>
    /// <returns>MemberApproval instance</returns>
    public static MemberApproval CreatePending(Guid memberId, string reason)
    {
        return new MemberApproval
        {
            Id = Guid.NewGuid(),
            MemberId = memberId,
            Approved = false,
            Reason = reason,
            ApprovedBy = null, // Null indicates pending status
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a rejection record
    /// </summary>
    /// <param name="memberId">The member ID</param>
    /// <param name="reason">Reason for rejection</param>
    /// <param name="rejectedBy">Who rejected it</param>
    /// <returns>MemberApproval instance</returns>
    public static MemberApproval CreateRejection(Guid memberId, string reason, Guid? rejectedBy = null)
    {
        return new MemberApproval
        {
            Id = Guid.NewGuid(),
            MemberId = memberId,
            Approved = false,
            Reason = reason,
            ApprovedBy = rejectedBy,
            CreatedAt = DateTime.UtcNow
        };
    }
}
