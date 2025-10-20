using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sinema.Application.Abstractions.Authentication;
using Sinema.Application.DTOs.Auth;
using Sinema.Domain.Entities;
using Sinema.Infrastructure.Identity;
using Sinema.Infrastructure.Persistence;
using Sinema.Infrastructure.Persistence.Entities;
using System.Security.Claims;

namespace Sinema.Api.Controllers;

/// <summary>
/// Authentication and authorization endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ICurrentUser _currentUser;
    private readonly SinemaDbContext _context;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtTokenService jwtTokenService,
        ICurrentUser currentUser,
        SinemaDbContext context,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
        _currentUser = currentUser;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user account
    /// </summary>
    /// <param name="request">Registration details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication response with member status</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), 201)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken = default)
    {
        // Check if user already exists
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Registration Failed",
                Detail = "A user with this email already exists.",
                Status = 400
            });
        }

        // Check if member already exists
        var existingMember = await _context.Members.FirstOrDefaultAsync(m => m.Email == request.Email, cancellationToken);
        if (existingMember != null)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Registration Failed",
                Detail = "A member with this email already exists.",
                Status = 400
            });
        }

        // Create new user
        var user = new ApplicationUser
        {
            Email = request.Email,
            UserName = request.Email,
            DisplayName = request.DisplayName ?? request.Email,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            EmailConfirmed = true // Auto-confirm for development
        };

        if (!string.IsNullOrEmpty(request.PhoneNumber))
        {
            user.PhoneNumber = request.PhoneNumber;
        }

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Registration Failed",
                Detail = string.Join(", ", result.Errors.Select(e => e.Description)),
                Status = 400
            });
        }

        // Assign WebUye role
        await _userManager.AddToRoleAsync(user, "WebUye");

        // Create Member record and approval in transaction
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // Create Member record
            var newMember = new Member
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                FullName = user.DisplayName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                VipStatus = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Members.Add(newMember);
            await _context.SaveChangesAsync(cancellationToken);

            // Create pending approval
            var approval = MemberApproval.CreatePending(newMember.Id, "Yeni web üyesi - onay bekliyor");

            _context.MemberApprovals.Add(approval);
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        _logger.LogInformation("New user registered: {Email}", user.Email);

        // Find created member for token generation
        var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == user.Id, cancellationToken);

        // Generate tokens
        var roles = await _userManager.GetRolesAsync(user);
        var claims = await BuildUserClaimsAsync(user, member, roles);
        var accessToken = _jwtTokenService.GenerateAccessToken((object)user, claims);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        // Save refresh token
        var userRefreshToken = UserRefreshToken.Create(
            user.Id,
            refreshToken,
            _jwtTokenService.RefreshTokenLifetimeDays,
            GetDeviceInfo()
        );

        _context.UserRefreshTokens.Add(userRefreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = _jwtTokenService.AccessTokenLifetimeMinutes * 60,
            TokenType = "Bearer",
            MemberStatus = "PendingApproval"
        };

        return CreatedAtAction(nameof(GetUserInfo), response);
    }

    /// <summary>
    /// Authenticate user and return tokens
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication response with tokens</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !user.IsActive)
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Login Failed",
                Detail = "Invalid email or password.",
                Status = 401
            });
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
            {
                return Unauthorized(new ProblemDetails
                {
                    Title = "Account Locked",
                    Detail = "Account is temporarily locked due to multiple failed login attempts.",
                    Status = 401
                });
            }

            return Unauthorized(new ProblemDetails
            {
                Title = "Login Failed",
                Detail = "Invalid email or password.",
                Status = 401
            });
        }

        // Get user's member info if exists
        var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == user.Id, cancellationToken);

        // Build claims
        var roles = await _userManager.GetRolesAsync(user);
        var claims = await BuildUserClaimsAsync(user, member, roles);

        // Generate tokens
        var accessToken = _jwtTokenService.GenerateAccessToken((object)user, claims);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        // Revoke old refresh tokens for this user
        var oldTokens = await _context.UserRefreshTokens
            .Where(rt => rt.UserId == user.Id && rt.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var oldToken in oldTokens)
        {
            oldToken.Revoke();
        }

        // Save new refresh token
        var userRefreshToken = UserRefreshToken.Create(
            user.Id,
            refreshToken,
            _jwtTokenService.RefreshTokenLifetimeDays,
            GetDeviceInfo()
        );

        _context.UserRefreshTokens.Add(userRefreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User logged in: {Email}", user.Email);

        var response = new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = _jwtTokenService.AccessTokenLifetimeMinutes * 60,
            TokenType = "Bearer"
        };

        return Ok(response);
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    /// <param name="request">Refresh token request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>New authentication tokens</returns>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        // Find and validate refresh token
        var refreshToken = await _context.UserRefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken);

        if (refreshToken == null || !refreshToken.IsActive)
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Invalid Refresh Token",
                Detail = "The provided refresh token is invalid or expired.",
                Status = 401
            });
        }

        // Get user
        var user = await _userManager.FindByIdAsync(refreshToken.UserId.ToString());
        if (user == null || !user.IsActive)
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Invalid User",
                Detail = "User account is not found or inactive.",
                Status = 401
            });
        }

        // Get user's member info
        var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == user.Id, cancellationToken);

        // Build claims
        var roles = await _userManager.GetRolesAsync(user);
        var claims = await BuildUserClaimsAsync(user, member, roles);

        // Generate new tokens
        var newAccessToken = _jwtTokenService.GenerateAccessToken((object)user, claims);
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

        // Revoke old refresh token
        refreshToken.Revoke();

        // Create new refresh token
        var userRefreshToken = UserRefreshToken.Create(
            user.Id,
            newRefreshToken,
            _jwtTokenService.RefreshTokenLifetimeDays,
            GetDeviceInfo()
        );

        _context.UserRefreshTokens.Add(userRefreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Tokens refreshed for user: {Email}", user.Email);

        var response = new AuthResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresIn = _jwtTokenService.AccessTokenLifetimeMinutes * 60,
            TokenType = "Bearer"
        };

        return Ok(response);
    }

    /// <summary>
    /// Logout user by revoking refresh token
    /// </summary>
    /// <param name="request">Refresh token to revoke</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpPost("logout")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var refreshToken = await _context.UserRefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken);

        if (refreshToken != null && refreshToken.IsActive)
        {
            refreshToken.Revoke();
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User logged out, refresh token revoked");
        }

        return NoContent();
    }

    /// <summary>
    /// Get current user information
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current user information</returns>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserInfoResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public async Task<IActionResult> GetUserInfo(CancellationToken cancellationToken = default)
    {
        if (_currentUser.UserId == null)
        {
            return Unauthorized();
        }

        var user = await _userManager.FindByIdAsync(_currentUser.UserId.ToString()!);
        if (user == null)
        {
            return Unauthorized();
        }

        var member = await _context.Members.FirstOrDefaultAsync(m => m.UserId == user.Id, cancellationToken);

        var response = new UserInfoResponse
        {
            UserId = user.Id,
            Email = user.Email!,
            DisplayName = user.GetDisplayName(),
            Roles = _currentUser.Roles,
            MemberId = member?.Id,
            IsApproved = _currentUser.IsApprovedMember,
            IsVip = _currentUser.IsVipMember
        };

        return Ok(response);
    }

    private async Task<List<Claim>> BuildUserClaimsAsync(ApplicationUser user, Member? member, IList<string> roles)
    {
        var claims = new List<Claim>();

        // Add role claims
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Add member-specific claims if member exists
        if (member != null)
        {
            claims.Add(new Claim("member_id", member.Id.ToString()));
            claims.Add(new Claim("is_vip", member.VipStatus.ToString().ToLower()));

            // Admin users are automatically considered approved
            bool isApproved = false;
            if (roles.Contains("Admin"))
            {
                isApproved = true;
            }
            else
            {
                // Check if member is approved
                isApproved = await _context.MemberApprovals
                    .Where(a => a.MemberId == member.Id)
                    .AnyAsync(a => a.Approved);
            }

            claims.Add(new Claim("is_approved", isApproved.ToString().ToLower()));
        }

        return claims;
    }

    private string GetDeviceInfo()
    {
        var userAgent = HttpContext.Request.Headers.UserAgent.ToString();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        return $"{userAgent} | {ipAddress}";
    }
}
