using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Sinema.Domain.Interfaces.Repositories;
using Sinema.Domain.Interfaces.Services;
using Sinema.Infrastructure.Services;
using Sinema.Infrastructure.Persistence.Repositories;
using Sinema.Infrastructure.Persistence.Interceptors;

namespace Sinema.Infrastructure.DependencyInjection;

/// <summary>
/// Extension methods for registering settings and audit services
/// </summary>
public static class SettingsAndAuditRegistration
{
    /// <summary>
    /// Adds settings and audit services to the dependency injection container
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration instance</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddSettingsAndAudit(this IServiceCollection services, IConfiguration configuration)
    {
        // Register repositories
        services.AddScoped<ISettingsRepository, SettingsRepository>();
        services.AddScoped<ISettingsChangeLogRepository, SettingsChangeLogRepository>();
        services.AddScoped<IDeletionLogRepository, DeletionLogRepository>();

        // Register services
        services.AddScoped<ISettingsService, SettingsService>();
        services.AddScoped<ISoftDeleteService, SoftDeleteService>();
        services.AddScoped<IAuditLogger, AuditLogger>();

        // Register interceptors
        services.AddScoped<SoftDeleteSaveChangesInterceptor>();

        return services;
    }
}
