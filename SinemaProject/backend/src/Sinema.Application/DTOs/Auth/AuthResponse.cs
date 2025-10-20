namespace Sinema.Application.DTOs.Auth;

/// <summary>
/// Response model for authentication operations (login, refresh)
/// </summary>
public class AuthResponse
{
    /// <summary>
    /// JWT access token
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Refresh token for getting new access tokens
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Access token expiration time in seconds from now
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Token type (typically "Bearer")
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// User's member status (for registration responses)
    /// </summary>
    public string? MemberStatus { get; set; }
}
