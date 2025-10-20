namespace Sinema.Application.DTOs.Members;

/// <summary>
/// DTO for member information
/// </summary>
public class MemberDto
{
    /// <summary>
    /// Member ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Full name of the member
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Phone number
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Associated user ID (if member registered through web)
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// VIP status
    /// </summary>
    public bool VipStatus { get; set; }

    /// <summary>
    /// When the member was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the member was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Whether the member is approved
    /// </summary>
    public bool IsApproved { get; set; }

    /// <summary>
    /// Latest approval status
    /// </summary>
    public MemberApprovalDto? LatestApproval { get; set; }
}
