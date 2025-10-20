using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Identity;
using Sinema.Infrastructure.Persistence;
using Sinema.Infrastructure.Identity;

// Console app to add specific users
var builder = Host.CreateApplicationBuilder(args);

// Read configuration
var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("src/Sinema.Api/appsettings.Development.json")
    .Build();

// Add services
var connectionString = config.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<SinemaDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<SinemaDbContext>()
    .AddDefaultTokenProviders();

var app = builder.Build();

using var scope = app.Services.CreateScope();
var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
var context = scope.ServiceProvider.GetRequiredService<SinemaDbContext>();

Console.WriteLine("=== Adding New Users ===");

// Admin User
var adminEmail = "admin@admin.com";
var existingAdmin = await userManager.FindByEmailAsync(adminEmail);

if (existingAdmin == null)
{
    var adminUser = new ApplicationUser
    {
        Email = adminEmail,
        UserName = adminEmail,
        EmailConfirmed = true,
        DisplayName = "Admin Adminoglu",
        CreatedAt = DateTime.UtcNow,
        IsActive = true
    };

    var result = await userManager.CreateAsync(adminUser, "Admin12345");
    if (result.Succeeded)
    {
        await userManager.AddToRoleAsync(adminUser, "Admin");
        Console.WriteLine($"✅ Successfully created admin user: {adminEmail}");
    }
    else
    {
        Console.WriteLine($"❌ Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
    }
}
else
{
    Console.WriteLine($"ℹ️ Admin user {adminEmail} already exists");
}

// Gise Gorevlisi User
var giseEmail = "gise@gise.com";
var existingGise = await userManager.FindByEmailAsync(giseEmail);

if (existingGise == null)
{
    var giseUser = new ApplicationUser
    {
        Email = giseEmail,
        UserName = giseEmail,
        EmailConfirmed = true,
        DisplayName = "Gise Giseoglu",
        CreatedAt = DateTime.UtcNow,
        IsActive = true
    };

    var result = await userManager.CreateAsync(giseUser, "Gise12345");
    if (result.Succeeded)
    {
        await userManager.AddToRoleAsync(giseUser, "GiseGorevlisi");
        Console.WriteLine($"✅ Successfully created gise user: {giseEmail}");
    }
    else
    {
        Console.WriteLine($"❌ Failed to create gise user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
    }
}
else
{
    Console.WriteLine($"ℹ️ Gise user {giseEmail} already exists");
}

// List all users
Console.WriteLine("\n=== Current Users ===");
var allUsers = await userManager.Users.ToListAsync();
foreach (var user in allUsers)
{
    var roles = await userManager.GetRolesAsync(user);
    Console.WriteLine($"User: {user.Email} | Name: {user.DisplayName} | Roles: {string.Join(", ", roles)} | Active: {user.IsActive}");
}

Console.WriteLine("\n=== Done ===");
