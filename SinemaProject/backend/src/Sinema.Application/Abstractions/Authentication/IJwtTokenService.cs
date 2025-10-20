using System.Security.Claims;

namespace Sinema.Application.Abstractions.Authentication;

/// <summary>
/// Service for JWT token generation and validation
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates an access token for the specified user with additional claims
    /// </summary>
    /// <param name="user">User to generate token for</param>
    /// <param name="extraClaims">Additional claims to include in the token</param>
    /// <returns>JWT access token string</returns>
    string GenerateAccessToken(object user, IEnumerable<Claim> extraClaims);

    /// <summary>
    /// Generates a cryptographically secure refresh token
    /// </summary>
    /// <returns>Random refresh token string</returns>
    string GenerateRefreshToken();

    /// <summary>
    /// Gets the principal from an expired token (used for refresh token validation)
    /// </summary>
    /// <param name="token">Expired access token</param>
    /// <returns>ClaimsPrincipal or null if invalid</returns>
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);

    /// <summary>
    /// Validates a token and returns the principal
    /// </summary>
    /// <param name="token">Token to validate</param>
    /// <returns>ClaimsPrincipal or null if invalid</returns>
    ClaimsPrincipal? ValidateToken(string token);

    /// <summary>
    /// Gets the access token lifetime in minutes
    /// </summary>
    int AccessTokenLifetimeMinutes { get; }

    /// <summary>
    /// Gets the refresh token lifetime in days
    /// </summary>
    int RefreshTokenLifetimeDays { get; }
}
