namespace Sinema.Infrastructure.Auth;

/// <summary>
/// Configuration options for JWT token generation and validation
/// </summary>
public class JwtOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json
    /// </summary>
    public const string SectionName = "Jwt";

    /// <summary>
    /// JWT issuer (iss claim)
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// JWT audience (aud claim)
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Signing key for JWT tokens (should be at least 32 characters)
    /// </summary>
    public string SigningKey { get; set; } = string.Empty;

    /// <summary>
    /// Access token lifetime in minutes
    /// </summary>
    public int AccessTokenLifetimeMinutes { get; set; } = 15;

    /// <summary>
    /// Refresh token lifetime in days
    /// </summary>
    public int RefreshTokenLifetimeDays { get; set; } = 14;

    /// <summary>
    /// Validates the JWT options configuration
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrEmpty(Issuer))
        {
            throw new InvalidOperationException("JWT Issuer is required");
        }

        if (string.IsNullOrEmpty(Audience))
        {
            throw new InvalidOperationException("JWT Audience is required");
        }

        if (string.IsNullOrEmpty(SigningKey))
        {
            throw new InvalidOperationException("JWT SigningKey is required");
        }

        if (SigningKey.Length < 32)
        {
            throw new InvalidOperationException("JWT SigningKey must be at least 32 characters long");
        }

        if (AccessTokenLifetimeMinutes <= 0)
        {
            throw new InvalidOperationException("Access token lifetime must be positive");
        }

        if (RefreshTokenLifetimeDays <= 0)
        {
            throw new InvalidOperationException("Refresh token lifetime must be positive");
        }
    }
}
