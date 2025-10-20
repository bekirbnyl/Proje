namespace Sinema.Api.Extensions;

/// <summary>
/// Extension methods for authorization configuration
/// </summary>
public static class AuthExtensions
{

    /// <summary>
    /// Adds authorization policies
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // Policy for approved members only
            options.AddPolicy("ApprovedMemberOnly", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim("is_approved", "true"));

            // Policy for viewing reports
            options.AddPolicy("CanViewReports", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireRole("Admin", "Patron", "Yonetim", "DepartmanMuduru", "SinemaMuduru"));

            // Policy for soft delete operations
            options.AddPolicy("CanSoftDelete", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireRole("Admin", "DepartmanMuduru", "SinemaMuduru", "GiseAmiri"));

            // Policy for VIP operations
            options.AddPolicy("VipMemberOnly", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireClaim("is_vip", "true"));

            // Policy for administrative operations
            options.AddPolicy("AdminOnly", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireRole("Admin"));

            // Policy for management operations
            options.AddPolicy("ManagementOnly", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireRole("Admin", "Patron", "Yonetim", "DepartmanMuduru", "SinemaMuduru"));

            // Policy for ticketing operations
            options.AddPolicy("TicketingStaff", policy =>
                policy.RequireAuthenticatedUser()
                      .RequireRole("Admin", "GiseAmiri", "GiseGorevlisi"));
        });

        return services;
    }
}
