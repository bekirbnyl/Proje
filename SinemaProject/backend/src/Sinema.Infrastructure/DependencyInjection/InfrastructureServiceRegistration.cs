using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sinema.Application.Abstractions.Export;
using Sinema.Application.Abstractions.Idempotency;
using Sinema.Application.Abstractions.Payments;
using Sinema.Application.Abstractions.Pricing;
using Sinema.Application.Abstractions.Reporting;
using Sinema.Application.Abstractions.Scheduling;
using Sinema.Application.Abstractions.Seating;
using Sinema.Application.Abstractions.Settings;
using Sinema.Application.Abstractions.Tickets;
using Sinema.Application.DTOs.Reports.Deletions;
using Sinema.Application.DTOs.Reports.Memberships;
using Sinema.Application.DTOs.Reports.Sales;
using Sinema.Domain.Interfaces.Repositories;
using Sinema.Domain.Interfaces.Services;
using Sinema.Infrastructure.Persistence;
using Sinema.Infrastructure.Persistence.Repositories;
using Sinema.Infrastructure.Persistence.Seed;
using Sinema.Infrastructure.Services.Clock;
using Sinema.Infrastructure.Services.Export.Excel;
using Sinema.Infrastructure.Services.Idempotency;
using Sinema.Infrastructure.Services.Payments;
using Sinema.Infrastructure.Services.Pricing;
using Sinema.Infrastructure.Services.Reporting;
using Sinema.Infrastructure.Services.Reservations;
using Sinema.Infrastructure.Services.Scheduling;
using Sinema.Infrastructure.Services.Seating;
using Sinema.Infrastructure.Services.Configuration;
using Sinema.Infrastructure.Services.Settings;
using Sinema.Infrastructure.Services.Tickets;

namespace Sinema.Infrastructure.DependencyInjection;

/// <summary>
/// Infrastructure layer dependency injection registration
/// </summary>
public static class InfrastructureServiceRegistration
{
    /// <summary>
    /// Add Infrastructure layer services to the dependency injection container
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database configuration
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

        // System services
        services.AddSingleton<IClock, SystemClock>();
        services.AddMemoryCache(); // Required for IdempotencyStore

        // Seating services
        services.AddScoped<ISeatStatusService, SeatStatusService>();

        // Scheduling services
        services.AddScoped<IScreeningSchedulingService, ScreeningSchedulingService>();
        services.AddScoped<ISpecialDayService, SpecialDayService>();

        // Pricing services
        services.AddScoped<IVipUsageService, VipUsageService>();

        // Payment services
        services.AddScoped<IPaymentGateway, PaymentGatewayStub>();

        // Ticket services
        services.AddSingleton<ITicketNumberGenerator, TicketNumberGenerator>();

        // Idempotency services
        services.AddScoped<IIdempotencyStore, IdempotencyStore>();

        // Settings services
        services.AddScoped<IAppSettingsReader, AppSettingsReader>();

        // Seat hold services
        services.AddScoped<ISeatHoldService, SeatHoldService>();

        // Register MediatR handlers
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(InfrastructureServiceRegistration).Assembly));

        // Data seeding service
        services.AddScoped<DataSeeder>();

        // Repository registrations
        services.AddScoped<IMovieRepository, MovieRepository>();
        services.AddScoped<IScreeningRepository, ScreeningRepository>();
        services.AddScoped<IHallRepository, HallRepository>();
        services.AddScoped<ISeatLayoutRepository, SeatLayoutRepository>();
        services.AddScoped<ISeatRepository, SeatRepository>();
        services.AddScoped<IDeletionLogRepository, DeletionLogRepository>();
        services.AddScoped<ISettingsRepository, SettingsRepository>();
        services.AddScoped<IMemberRepository, MemberRepository>();
        services.AddScoped<IMemberApprovalRepository, MemberApprovalRepository>();
        services.AddScoped<ISeatHoldRepository, SeatHoldRepository>();
        
        services.AddScoped<IReservationRepository, ReservationRepository>();
        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        // Reporting services
        services.AddScoped<IReportBuilder<SalesReportRequest, SalesReportResponse>, SalesReportBuilder>();
        services.AddScoped<IReportBuilder<DeletionsReportRequest, DeletionsReportResponse>, DeletionsReportBuilder>();
        services.AddScoped<IReportBuilder<MembershipsReportRequest, MembershipsReportResponse>, MembershipsReportBuilder>();
        services.AddScoped<IExcelExporter, ExcelExporter>();

        // Configure report options
        services.Configure<ReportsOptions>(configuration.GetSection("Reports"));
        
        // TODO: Register other repositories as they are implemented
        // services.AddScoped<ICinemaRepository, CinemaRepository>();

        return services;
    }
}
