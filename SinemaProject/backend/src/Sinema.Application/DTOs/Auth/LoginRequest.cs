namespace Sinema.Application.DTOs.Auth;

/// <summary>
/// Request model for user login
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// User's email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's password
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Remember me flag for extended session (optional)
    /// </summary>
    public bool RememberMe { get; set; } = false;
}
