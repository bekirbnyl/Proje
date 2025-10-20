using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Sinema.Application.Abstractions.Export;
using Sinema.Application.Abstractions.Pricing;
using Sinema.Application.Abstractions.Reporting;
using Sinema.Application.Common.Behaviors;
using Sinema.Application.DTOs.Reports.Deletions;
using Sinema.Application.DTOs.Reports.Memberships;
using Sinema.Application.DTOs.Reports.Sales;
using Sinema.Application.Policies.Pricing;
using Sinema.Application.Policies.Pricing.Rules;
using System.Reflection;

namespace Sinema.Application.Extensions;

/// <summary>
/// Application layer service registration extensions
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add Application layer services
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        });

        // Register validators
        services.AddValidatorsFromAssembly(assembly, ServiceLifetime.Scoped);
        
        // Register pricing services
        services.AddPricingServices();
        
        // Register reporting services
        services.AddReportingServices();
        
        services.AddMapster();

        return services;
    }

    /// <summary>
    /// Add Mapster configuration
    /// </summary>
    private static IServiceCollection AddMapster(this IServiceCollection services)
    {
        var config = TypeAdapterConfig.GlobalSettings;
        
        // Configure Mapster mappings here if needed
        // config.NewConfig<SourceType, DestinationType>()
        //       .Map(dest => dest.Property, src => src.SourceProperty);

        services.AddSingleton(config);
        return services;
    }

    /// <summary>
    /// Add pricing-related services
    /// </summary>
    private static IServiceCollection AddPricingServices(this IServiceCollection services)
    {
        // Register pricing engine
        services.AddScoped<IPricingEngine, PricingEngine>();

        // Register all pricing rules
        services.AddScoped<IPricingRule, HalkGunuRule>();
        services.AddScoped<IPricingRule, FirstWeekdayShowRule>();
        services.AddScoped<IPricingRule, StudentDiscountRule>();
        services.AddScoped<IPricingRule, VipMonthlyFreeRule>();
        services.AddScoped<IPricingRule, VipAdditionalMovieRule>();
        services.AddScoped<IPricingRule, VipGuestDiscountRule>();

        return services;
    }

    /// <summary>
    /// Add reporting-related services
    /// </summary>
    private static IServiceCollection AddReportingServices(this IServiceCollection services)
    {
        // Note: Report builders and Excel exporter are registered in Infrastructure layer
        // This method is reserved for any Application-specific reporting services
        
        return services;
    }
}
