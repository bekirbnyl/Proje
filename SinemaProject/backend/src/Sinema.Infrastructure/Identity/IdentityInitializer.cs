using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sinema.Domain.Entities;
using Sinema.Infrastructure.Persistence;

namespace Sinema.Infrastructure.Identity;

/// <summary>
/// Initializes Identity roles and users for development/seeding
/// </summary>
public static class IdentityInitializer
{
    /// <summary>
    /// Seeds roles and development users (idempotent operation)
    /// </summary>
    /// <param name="serviceProvider">Service provider</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public static async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("IdentityInitializer");
        
        try
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = scope.ServiceProvider.GetRequiredService<SinemaDbContext>();

            // Seed roles
            await SeedRolesAsync(roleManager, logger, cancellationToken);

            // Seed development users
            await SeedUsersAsync(userManager, context, logger, cancellationToken);

            logger.LogInformation("Identity initialization completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing Identity");
            throw;
        }
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, ILogger logger, CancellationToken cancellationToken)
    {
        var roles = new[]
        {
            "Admin",
            "Patron",
            "Yonetim", 
            "DepartmanMuduru",
            "SinemaMuduru",
            "GiseGorevlisi",
            "GiseAmiri",
            "WebUye",
            "WebUyeVIP"
        };

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var role = new IdentityRole
                {
                    Name = roleName,
                    NormalizedName = roleName.ToUpperInvariant()
                };

                var result = await roleManager.CreateAsync(role);
                if (result.Succeeded)
                {
                    logger.LogInformation("Created role: {RoleName}", roleName);
                }
                else
                {
                    logger.LogError("Failed to create role {RoleName}: {Errors}", roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }

    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager, SinemaDbContext context, ILogger logger, CancellationToken cancellationToken)
    {
        var users = new[]
        {
            new { Email = "admin@cinema.local", Password = "Admin*123", DisplayName = "System Administrator", Role = "Admin" },
            new { Email = "testadminbekir@cinema.local", Password = "TestAdmin123!", DisplayName = "Test Administrator", Role = "Admin" },
            new { Email = "gise@cinema.local", Password = "Gise*123", DisplayName = "Gişe Görevlisi", Role = "GiseGorevlisi" },
            new { Email = "yonetim@cinema.local", Password = "Yonetim*123", DisplayName = "Yönetim", Role = "Yonetim" },
            new { Email = "uye@cinema.local", Password = "Uye*123", DisplayName = "Test Üyesi", Role = "WebUye" }
        };

        foreach (var userData in users)
        {
            var existingUser = await userManager.FindByEmailAsync(userData.Email);
            if (existingUser == null)
            {
                var user = new ApplicationUser
                {
                    Email = userData.Email,
                    UserName = userData.Email,
                    EmailConfirmed = true,
                    DisplayName = userData.DisplayName,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                var result = await userManager.CreateAsync(user, userData.Password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, userData.Role);
                    logger.LogInformation("Created user: {Email} with role: {Role}", userData.Email, userData.Role);

                    // Create Member record for WebUye users
                    if (userData.Role == "WebUye")
                    {
                        await CreateMemberForUserAsync(context, user, logger, cancellationToken);
                    }
                }
                else
                {
                    logger.LogError("Failed to create user {Email}: {Errors}", userData.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                logger.LogDebug("User {Email} already exists, skipping", userData.Email);
            }
        }
    }

    private static async Task CreateMemberForUserAsync(SinemaDbContext context, ApplicationUser user, ILogger logger, CancellationToken cancellationToken)
    {
        // Check if member already exists for this user
        var existingMember = await context.Members.FirstOrDefaultAsync(m => m.UserId == user.Id, cancellationToken);
        if (existingMember != null)
        {
            return;
        }

        var member = new Member
        {
            UserId = user.Id,
            FullName = user.DisplayName,
            Email = user.Email!,
            VipStatus = false,
            CreatedAt = DateTime.UtcNow
        };

        context.Members.Add(member);

        // Create pending approval for web member
        var approval = MemberApproval.CreateRejection(
            member.Id,
            "Yeni web üyesi - onay bekliyor",
            null
        );
        approval.Approved = false; // Set as pending (not approved yet)

        context.MemberApprovals.Add(approval);

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Created Member record for user {Email} with pending approval", user.Email);
    }
}
