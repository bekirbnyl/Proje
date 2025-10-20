using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Sinema.Application.Abstractions.Authentication;
using Sinema.Infrastructure.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Sinema.Infrastructure.Auth;

/// <summary>
/// Service for generating and validating JWT tokens
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _jwtOptions;
    private readonly TokenValidationParameters _tokenValidationParameters;

    public JwtTokenService(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
        _jwtOptions.Validate();

        _tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _jwtOptions.Issuer,
            ValidAudience = _jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey)),
            ClockSkew = TimeSpan.FromSeconds(30) // Reduced clock skew for security
        };
    }

    /// <summary>
    /// Generates an access token for the specified user with claims
    /// </summary>
    /// <param name="user">User to generate token for</param>
    /// <param name="extraClaims">Additional claims to include</param>
    /// <returns>JWT access token</returns>
    public string GenerateAccessToken(object user, IEnumerable<Claim> extraClaims)
    {
        if (user is not ApplicationUser appUser)
        {
            throw new ArgumentException("User must be of type ApplicationUser", nameof(user));
        }

        return GenerateAccessTokenForUser(appUser, extraClaims);
    }

    /// <summary>
    /// Generates an access token for the specified ApplicationUser with claims
    /// </summary>
    /// <param name="user">ApplicationUser to generate token for</param>
    /// <param name="extraClaims">Additional claims to include</param>
    /// <returns>JWT access token</returns>
    public string GenerateAccessTokenForUser(ApplicationUser user, IEnumerable<Claim> extraClaims)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()), // Add NameIdentifier for controller compatibility
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty), // Add standard email claim
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new("display_name", user.GetDisplayName()),
            new(ClaimTypes.Name, user.Email ?? string.Empty) // Add name claim
        };

        // Add extra claims (roles, member info, etc.)
        if (extraClaims != null)
        {
            claims.AddRange(extraClaims);
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenLifetimeMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Generates a cryptographically secure refresh token
    /// </summary>
    /// <returns>Random refresh token string</returns>
    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    /// <summary>
    /// Gets the principal from an expired token (for refresh token validation)
    /// </summary>
    /// <param name="token">Expired access token</param>
    /// <returns>ClaimsPrincipal or null if invalid</returns>
    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            // Create validation parameters without lifetime validation for expired tokens
            var validationParameters = _tokenValidationParameters.Clone();
            validationParameters.ValidateLifetime = false;

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

            // Verify it's a JWT token with the correct algorithm
            if (validatedToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Validates a token and returns the principal
    /// </summary>
    /// <param name="token">Token to validate</param>
    /// <returns>ClaimsPrincipal or null if invalid</returns>
    public ClaimsPrincipal? ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            var principal = tokenHandler.ValidateToken(token, _tokenValidationParameters, out _);
            return principal;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Gets the access token lifetime in minutes
    /// </summary>
    public int AccessTokenLifetimeMinutes => _jwtOptions.AccessTokenLifetimeMinutes;

    /// <summary>
    /// Gets the refresh token lifetime in days
    /// </summary>
    public int RefreshTokenLifetimeDays => _jwtOptions.RefreshTokenLifetimeDays;
}
