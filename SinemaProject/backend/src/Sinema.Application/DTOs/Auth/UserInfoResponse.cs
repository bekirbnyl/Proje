namespace Sinema.Application.DTOs.Auth;

/// <summary>
/// Response model for user information (me endpoint)
/// </summary>
public class UserInfoResponse
{
    /// <summary>
    /// User's unique identifier
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// User's email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's display name
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// User's assigned roles
    /// </summary>
    public IEnumerable<string> Roles { get; set; } = new List<string>();

    /// <summary>
    /// Associated member ID (if user is a member)
    /// </summary>
    public Guid? MemberId { get; set; }

    /// <summary>
    /// Whether the member is approved (if user is a member)
    /// </summary>
    public bool IsApproved { get; set; }

    /// <summary>
    /// Whether the member has VIP status (if user is a member)
    /// </summary>
    public bool IsVip { get; set; }
}
