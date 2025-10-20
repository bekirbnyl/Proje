using Microsoft.AspNetCore.Identity;

namespace Sinema.Infrastructure.Identity;

/// <summary>
/// Application user entity extending IdentityUser with string primary key
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// Display name for the user (used in UI)
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// When the user was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether the user account is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets the user's display name or email as fallback
    /// </summary>
    public string GetDisplayName() => !string.IsNullOrEmpty(DisplayName) ? DisplayName : Email ?? "Unknown User";
}
