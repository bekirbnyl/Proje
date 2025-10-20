namespace Sinema.Application.DTOs.Members;

/// <summary>
/// DTO for member approval information
/// </summary>
public class MemberApprovalDto
{
    /// <summary>
    /// Approval ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Member ID
    /// </summary>
    public Guid MemberId { get; set; }

    /// <summary>
    /// Whether the member was approved
    /// </summary>
    public bool Approved { get; set; }

    /// <summary>
    /// Reason for approval/rejection
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// ID of the user who approved/rejected
    /// </summary>
    public Guid? ApprovedBy { get; set; }

    /// <summary>
    /// When the approval was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Member information
    /// </summary>
    public MemberDto? Member { get; set; }
}
