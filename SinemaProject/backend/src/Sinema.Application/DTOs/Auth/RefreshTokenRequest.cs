namespace Sinema.Application.DTOs.Auth;

/// <summary>
/// Request model for refresh token operations
/// </summary>
public class RefreshTokenRequest
{
    /// <summary>
    /// The refresh token to use for getting a new access token
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
}
