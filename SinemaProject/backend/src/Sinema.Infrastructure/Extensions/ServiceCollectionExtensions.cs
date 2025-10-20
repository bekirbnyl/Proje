using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sinema.Infrastructure.Persistence;
using Sinema.Infrastructure.Identity;
using Sinema.Infrastructure.DependencyInjection;

namespace Sinema.Infrastructure.Extensions;

/// <summary>
/// Infrastructure layer service registration extensions
/// </summary>
public static class ServiceCollectionExtensions
{
    // Infrastructure registration moved to InfrastructureServiceRegistration class

    /// <summary>
    /// Add Entity Framework and database services
    /// </summary>
    private static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("SQL Server connection string is not configured");

        services.AddDbContext<SinemaDbContext>(options =>
        {
            options.UseSqlServer(connectionString, b =>
            {
                b.MigrationsAssembly(typeof(SinemaDbContext).Assembly.FullName);
            });
            
            options.EnableSensitiveDataLogging(false);
            options.EnableDetailedErrors(false);
        });

        // Register repositories here when they are created
        // services.AddScoped<IRepository, Repository>();

        return services;
    }

    /// <summary>
    /// Add ASP.NET Core Identity services
    /// </summary>
    private static IServiceCollection AddIdentityServices(this IServiceCollection services)
    {
        // TODO: Configure Identity services when ready
        /*
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            // Password settings
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = true;
            options.Password.RequiredLength = 6;
            options.Password.RequiredUniqueChars = 1;

            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // User settings
            options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<SinemaDbContext>()
        .AddDefaultTokenProviders();
        */

        return services;
    }
}
