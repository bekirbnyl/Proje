using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Sinema.Application.Abstractions.Authentication;
using Sinema.Infrastructure.Auth;
using Sinema.Infrastructure.Identity;
using Sinema.Infrastructure.Persistence;
using System.Text;

namespace Sinema.Infrastructure.DependencyInjection;

/// <summary>
/// Service registration for Identity and Authentication services
/// </summary>
public static class IdentityServiceRegistration
{
    /// <summary>
    /// Adds Identity and JWT authentication services to the container
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure JWT options
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()!;
        jwtOptions.Validate();

        // Add Identity with string keys
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            // Password settings
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 8;
            options.Password.RequiredUniqueChars = 1;

            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // User settings
            options.User.RequireUniqueEmail = true;
            options.User.AllowedUserNameCharacters = string.Empty; // Allow any characters in username

            // Sign in settings
            options.SignIn.RequireConfirmedEmail = false; // Set to true in production
            options.SignIn.RequireConfirmedPhoneNumber = false;
        })
        .AddEntityFrameworkStores<SinemaDbContext>()
        .AddDefaultTokenProviders();

        // Add JWT Authentication
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false; // Set to true in production
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidAudience = jwtOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
                ClockSkew = TimeSpan.FromSeconds(30) // Reduced clock skew for security
            };
        });

        // Add Authorization with policies
        services.AddAuthorization(options =>
        {
            // Policy for approved members only
            options.AddPolicy("ApprovedMemberOnly", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim("is_approved", "true"));

            // Policy for viewing reports
            options.AddPolicy("CanViewReports", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireRole("Admin", "Yonetim", "SinemaMuduru"));

            // Policy for soft delete operations
            options.AddPolicy("CanSoftDelete", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireRole("Admin", "GiseAmiri"));

            // Policy for VIP operations
            options.AddPolicy("VipMemberOnly", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim("is_vip", "true"));
        });

        // Register authentication services
        services.AddHttpContextAccessor();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<ICurrentUser, CurrentUser>();

        return services;
    }
}
