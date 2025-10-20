using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Identity;
using Sinema.Infrastructure.Persistence;
using Sinema.Infrastructure.Identity;

// Simple console app to check and create users
var builder = Host.CreateApplicationBuilder(args);

// Read configuration
var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("src/Sinema.Api/appsettings.Development.json")
    .Build();

// Add services
var connectionString = config.GetConnectionString("SqlServer");
builder.Services.AddDbContext<SinemaDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<SinemaDbContext>()
    .AddDefaultTokenProviders();

var app = builder.Build();

using var scope = app.Services.CreateScope();
var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
var context = scope.ServiceProvider.GetRequiredService<SinemaDbContext>();

Console.WriteLine("=== Checking Users in Database ===");

// Check existing users
var users = await userManager.Users.ToListAsync();
Console.WriteLine($"Total users found: {users.Count}");

foreach (var user in users)
{
    var roles = await userManager.GetRolesAsync(user);
    Console.WriteLine($"User: {user.Email} | Roles: {string.Join(", ", roles)} | Active: {user.IsActive}");
}

Console.WriteLine("\n=== Checking Roles ===");
var allRoles = await roleManager.Roles.ToListAsync();
foreach (var role in allRoles)
{
    Console.WriteLine($"Role: {role.Name}");
}

// Try to create the test admin user manually
Console.WriteLine("\n=== Creating Test Admin User ===");
var testEmail = "testadminbekir@cinema.local";
var existingTestUser = await userManager.FindByEmailAsync(testEmail);

if (existingTestUser == null)
{
    var testUser = new ApplicationUser
    {
        Email = testEmail,
        UserName = testEmail,
        EmailConfirmed = true,
        DisplayName = "Test Administrator",
        CreatedAt = DateTime.UtcNow,
        IsActive = true
    };

    var result = await userManager.CreateAsync(testUser, "TestAdmin123!");
    if (result.Succeeded)
    {
        await userManager.AddToRoleAsync(testUser, "Admin");
        Console.WriteLine($"✅ Successfully created user: {testEmail}");
    }
    else
    {
        Console.WriteLine($"❌ Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
    }
}
else
{
    Console.WriteLine($"ℹ️ User {testEmail} already exists");
    var roles = await userManager.GetRolesAsync(existingTestUser);
    Console.WriteLine($"Current roles: {string.Join(", ", roles)}");
}
