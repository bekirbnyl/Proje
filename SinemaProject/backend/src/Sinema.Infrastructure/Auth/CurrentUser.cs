using Microsoft.AspNetCore.Http;
using Sinema.Application.Abstractions.Authentication;
using System.Security.Claims;

namespace Sinema.Infrastructure.Auth;

/// <summary>
/// Service for accessing current user information from HTTP context
/// </summary>
public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Gets the current user's ID
    /// </summary>
    public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;

    /// <summary>
    /// Gets the current user's email
    /// </summary>
    public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value
                            ?? _httpContextAccessor.HttpContext?.User?.FindFirst("email")?.Value;

    /// <summary>
    /// Gets the current user's display name
    /// </summary>
    public string? DisplayName => _httpContextAccessor.HttpContext?.User?.FindFirst("display_name")?.Value;

    /// <summary>
    /// Gets the current user's roles
    /// </summary>
    public IEnumerable<string> Roles => _httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role)
                                                             ?.Select(c => c.Value) ?? Enumerable.Empty<string>();

    /// <summary>
    /// Checks if the current user is authenticated
    /// </summary>
    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;

    /// <summary>
    /// Checks if the current user has the specified role
    /// </summary>
    /// <param name="role">Role to check</param>
    /// <returns>True if user has the role</returns>
    public bool IsInRole(string role) => _httpContextAccessor.HttpContext?.User?.IsInRole(role) == true;

    /// <summary>
    /// Gets the current user's member ID (if they have one)
    /// </summary>
    public Guid? MemberId
    {
        get
        {
            var memberIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("member_id")?.Value;
            return Guid.TryParse(memberIdClaim, out var memberId) ? memberId : null;
        }
    }

    /// <summary>
    /// Checks if the current user is an approved member
    /// Admin users are automatically considered approved
    /// </summary>
    public bool IsApprovedMember => _httpContextAccessor.HttpContext?.User?.HasClaim("is_approved", "true") == true 
                                    || _httpContextAccessor.HttpContext?.User?.IsInRole("Admin") == true;

    /// <summary>
    /// Checks if the current user is a VIP member
    /// </summary>
    public bool IsVipMember => _httpContextAccessor.HttpContext?.User?.HasClaim("is_vip", "true") == true;

    /// <summary>
    /// Gets a specific claim value
    /// </summary>
    /// <param name="claimType">Claim type</param>
    /// <returns>Claim value or null</returns>
    public string? GetClaim(string claimType) => _httpContextAccessor.HttpContext?.User?.FindFirst(claimType)?.Value;

    /// <summary>
    /// Gets all claims for the current user
    /// </summary>
    public IEnumerable<Claim> GetClaims() => _httpContextAccessor.HttpContext?.User?.Claims ?? Enumerable.Empty<Claim>();
}
