using Serilog;
using Sinema.Api.Extensions;
using Sinema.Application.Extensions;
using Sinema.Infrastructure.Extensions;
using Sinema.Infrastructure.DependencyInjection;
using Sinema.Infrastructure.Identity;
using Sinema.Background.Schedulers;
using Sinema.Infrastructure.Persistence;
using Sinema.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Serilog Configuration
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add custom service registrations
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddSettingsAndAudit(builder.Configuration);
builder.Services.AddApi(builder.Configuration);
// Temporarily disable Quartz for testing
// builder.Services.AddQuartzBackgroundJobs(builder.Configuration);

var app = builder.Build();

// Database initialization and seeding in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<SinemaDbContext>();
        var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();

        // Ensure database is created and migrated
        await context.Database.MigrateAsync();

        // Seed initial data
        await seeder.SeedAsync(context);

        // Seed Identity data
        await IdentityInitializer.SeedAsync(scope.ServiceProvider);

        app.Logger.LogInformation("Database initialized and seeded successfully");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "An error occurred while initializing the database");
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sinema API V1");
        c.RoutePrefix = string.Empty; // Swagger UI at app root
    });
}

app.UseSerilogRequestLogging();
app.UseExceptionHandling();

// Enable CORS
app.UseCors("AllowFrontend");

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

// Make Program class public for integration tests
public partial class Program { }