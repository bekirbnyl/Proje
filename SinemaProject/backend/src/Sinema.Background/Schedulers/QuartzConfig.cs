using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Sinema.Background.Jobs;

namespace Sinema.Background.Schedulers;

/// <summary>
/// Configuration for Quartz background jobs
/// </summary>
public static class QuartzConfig
{
    /// <summary>
    /// Configures Quartz services and schedules background jobs
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddQuartzBackgroundJobs(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddQuartz(q =>
        {
            // Use a scoped job factory to enable dependency injection
            q.UseMicrosoftDependencyInjectionJobFactory();

            // Configure SeatHoldsCleanupJob to run every minute
            var seatHoldsCleanupJobKey = new JobKey("SeatHoldsCleanup");
            q.AddJob<SeatHoldsCleanupJob>(opts => opts.WithIdentity(seatHoldsCleanupJobKey));
            q.AddTrigger(opts => opts
                .ForJob(seatHoldsCleanupJobKey)
                .WithIdentity("SeatHoldsCleanup-trigger")
                .WithCronSchedule("0 * * * * ?") // Every minute
                .WithDescription("Cleans up expired seat holds every minute"));

            // Configure ReservationsExpireJob to run every 5 minutes
            var reservationsExpireJobKey = new JobKey("ReservationsExpire");
            q.AddJob<ReservationsExpireJob>(opts => opts.WithIdentity(reservationsExpireJobKey));
            q.AddTrigger(opts => opts
                .ForJob(reservationsExpireJobKey)
                .WithIdentity("ReservationsExpire-trigger")
                .WithCronSchedule("0 */5 * * * ?") // Every 5 minutes
                .WithDescription("Expires pending reservations based on T-30 rule every 5 minutes"));

            // Configure GenerateScheduledReportsJob if enabled
            var quartzConfig = configuration.GetSection("Quartz");
            var isEnabled = quartzConfig.GetValue<bool>("Enabled", false);
            
            if (isEnabled)
            {
                var salesDailyCron = quartzConfig.GetValue<string>("SalesDailyCron", "0 0 8 * * ?"); // Daily at 8 AM
                
                var reportsJobKey = new JobKey("GenerateScheduledReports");
                q.AddJob<GenerateScheduledReportsJob>(opts => opts.WithIdentity(reportsJobKey));
                q.AddTrigger(opts => opts
                    .ForJob(reportsJobKey)
                    .WithIdentity("GenerateScheduledReports-trigger")
                    .WithCronSchedule(salesDailyCron)
                    .WithDescription("Generates scheduled reports based on configuration"));
            }
        });

        // Add Quartz hosted service
        services.AddQuartzHostedService(q =>
        {
            // Gracefully shutdown jobs when application stops
            q.WaitForJobsToComplete = true;
        });

        return services;
    }
}
