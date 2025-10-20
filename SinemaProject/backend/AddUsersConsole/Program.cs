using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Identity;
using Sinema.Infrastructure.Persistence;
using Sinema.Infrastructure.Identity;
using Sinema.Domain.Entities;

// Console app to add specific users
var builder = Host.CreateApplicationBuilder(args);

// Add services - using MSSQL
var connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=SinemaDb_Dev;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true";
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

Console.WriteLine("=== Adding New Users ===");

// Check if roles exist first
var roles = new[] { "Admin", "GiseGorevlisi" };
foreach (var roleName in roles)
{
    if (!await roleManager.RoleExistsAsync(roleName))
    {
        var role = new IdentityRole(roleName);
        await roleManager.CreateAsync(role);
        Console.WriteLine($"✅ Created role: {roleName}");
    }
}

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

    var result = await userManager.CreateAsync(adminUser, "Admin123!");
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

    var result = await userManager.CreateAsync(giseUser, "Gise123!");
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

// Create Member records for Admin and GiseGorevlisi users
Console.WriteLine("\n=== Adding Member Records ===");

// Find admin users without Member records
var adminUsers = await userManager.GetUsersInRoleAsync("Admin");

foreach (var adminUser in adminUsers)
{
    // Check if Member record exists
    var existingMember = await context.Members.FirstOrDefaultAsync(m => m.UserId == adminUser.Id);
    
    if (existingMember == null)
    {
        // Create Member record for admin
        var member = new Member
        {
            Id = Guid.NewGuid(),
            UserId = adminUser.Id,
            FullName = adminUser.DisplayName,
            Email = adminUser.Email!,
            PhoneNumber = adminUser.PhoneNumber,
            VipStatus = true, // Give VIP status to admin
            CreatedAt = DateTime.UtcNow
        };

        context.Members.Add(member);
        await context.SaveChangesAsync();

        // Create an approved MemberApproval record
        var approval = MemberApproval.CreateApproval(member.Id, "Admin kullanıcısı - otomatik onay", Guid.NewGuid());

        context.MemberApprovals.Add(approval);
        await context.SaveChangesAsync();

        Console.WriteLine($"✅ Created Member record for admin: {adminUser.Email}");
    }
    else
    {
        Console.WriteLine($"ℹ️ Member record already exists for: {adminUser.Email}");
    }
}

// Also add for GiseGorevlisi users
var giseUsers = await userManager.GetUsersInRoleAsync("GiseGorevlisi");

foreach (var giseUser in giseUsers)
{
    // Check if Member record exists
    var existingMember = await context.Members.FirstOrDefaultAsync(m => m.UserId == giseUser.Id);
    
    if (existingMember == null)
    {
        // Create Member record for gise user
        var member = new Member
        {
            Id = Guid.NewGuid(),
            UserId = giseUser.Id,
            FullName = giseUser.DisplayName,
            Email = giseUser.Email!,
            PhoneNumber = giseUser.PhoneNumber,
            VipStatus = false,
            CreatedAt = DateTime.UtcNow
        };

        context.Members.Add(member);
        await context.SaveChangesAsync();

        // Create an approved MemberApproval record
        var approval = MemberApproval.CreateApproval(member.Id, "Gişe görevlisi - otomatik onay", Guid.NewGuid());

        context.MemberApprovals.Add(approval);
        await context.SaveChangesAsync();

        Console.WriteLine($"✅ Created Member record for gise user: {giseUser.Email}");
    }
    else
    {
        Console.WriteLine($"ℹ️ Member record already exists for: {giseUser.Email}");
    }
}

Console.WriteLine("\n=== Current Users ===");
var allUsers = await userManager.Users.ToListAsync();
foreach (var user in allUsers)
{
    var roles_user = await userManager.GetRolesAsync(user);
    var memberRecord = await context.Members.FirstOrDefaultAsync(m => m.UserId == user.Id);
    Console.WriteLine($"User: {user.Email} | Name: {user.DisplayName} | Roles: {string.Join(", ", roles_user)} | Active: {user.IsActive} | Member: {(memberRecord != null ? "Yes" : "No")}");
}

Console.WriteLine("\n=== Done ===");
