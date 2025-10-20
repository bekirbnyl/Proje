using System.Security.Claims;

namespace Sinema.Application.Abstractions.Authentication;

/// <summary>
/// Service for accessing current user information
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// Gets the current user's ID
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Gets the current user's email
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Gets the current user's display name
    /// </summary>
    string? DisplayName { get; }

    /// <summary>
    /// Gets the current user's roles
    /// </summary>
    IEnumerable<string> Roles { get; }

    /// <summary>
    /// Checks if the current user is authenticated
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Checks if the current user has the specified role
    /// </summary>
    /// <param name="role">Role to check</param>
    /// <returns>True if user has the role</returns>
    bool IsInRole(string role);

    /// <summary>
    /// Gets the current user's member ID (if they have one)
    /// </summary>
    Guid? MemberId { get; }

    /// <summary>
    /// Checks if the current user is an approved member
    /// </summary>
    bool IsApprovedMember { get; }

    /// <summary>
    /// Checks if the current user is a VIP member
    /// </summary>
    bool IsVipMember { get; }

    /// <summary>
    /// Gets a specific claim value
    /// </summary>
    /// <param name="claimType">Claim type</param>
    /// <returns>Claim value or null</returns>
    string? GetClaim(string claimType);

    /// <summary>
    /// Gets all claims for the current user
    /// </summary>
    IEnumerable<Claim> GetClaims();
}
