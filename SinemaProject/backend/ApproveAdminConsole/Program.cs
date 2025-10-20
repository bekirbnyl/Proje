using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Identity;
using Sinema.Infrastructure.Persistence;
using Sinema.Infrastructure.Identity;
using Sinema.Domain.Entities;

// Console app to approve admin user
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
var context = scope.ServiceProvider.GetRequiredService<SinemaDbContext>();

Console.WriteLine("=== Approving Admin User ===\n");

// Find admin user
var adminEmail = "admin@cinema.local";
var adminUser = await userManager.FindByEmailAsync(adminEmail);

if (adminUser == null)
{
    Console.WriteLine($"❌ Admin user with email {adminEmail} not found!");
    Console.WriteLine("\nSearching for users with Admin role...");
    
    // Try to find any admin users
    var adminUsers = await userManager.GetUsersInRoleAsync("Admin");
    if (adminUsers.Any())
    {
        Console.WriteLine($"\n✅ Found {adminUsers.Count} user(s) with Admin role:");
        foreach (var admin in adminUsers)
        {
            Console.WriteLine($"  - {admin.Email}");
        }
        
        adminUser = adminUsers.First();
        Console.WriteLine($"\nUsing first admin user: {adminUser.Email}");
    }
    else
    {
        Console.WriteLine("❌ No admin users found in the system!");
        return;
    }
}
else
{
    Console.WriteLine($"✅ Found admin user: {adminUser.Email}");
}

// Check if user has a member record
var member = await context.Members.FirstOrDefaultAsync(m => m.UserId == adminUser.Id);

if (member == null)
{
    Console.WriteLine($"\n⚠️ No member record found for admin user. Creating one...");
    
    // Create member record
    member = new Member
    {
        Id = Guid.NewGuid(),
        UserId = adminUser.Id,
        FullName = adminUser.DisplayName ?? "Admin User",
        Email = adminUser.Email!,
        PhoneNumber = adminUser.PhoneNumber,
        VipStatus = false,
        CreatedAt = DateTime.UtcNow
    };
    
    context.Members.Add(member);
    await context.SaveChangesAsync();
    Console.WriteLine($"✅ Created member record for admin user (ID: {member.Id})");
}
else
{
    Console.WriteLine($"✅ Found member record: {member.FullName} (ID: {member.Id})");
}

// Check existing approvals
var existingApprovals = await context.MemberApprovals
    .Where(a => a.MemberId == member.Id)
    .ToListAsync();

Console.WriteLine($"\n📋 Existing approvals: {existingApprovals.Count}");
if (existingApprovals.Any())
{
    foreach (var approval in existingApprovals)
    {
        Console.WriteLine($"  - ID: {approval.Id}");
        Console.WriteLine($"    Approved: {approval.Approved}");
        Console.WriteLine($"    Reason: {approval.Reason}");
        Console.WriteLine($"    Created: {approval.CreatedAt}");
        Console.WriteLine();
    }
}

// Check if already approved
var isApproved = existingApprovals.Any(a => a.Approved);

if (isApproved)
{
    Console.WriteLine($"✅ Admin user is already approved!");
}
else
{
    Console.WriteLine($"⚠️ Admin user is NOT approved. Approving now...");
    
    // Create approval record
    var approval = MemberApproval.CreateApproval(
        member.Id,
        "Admin user - automatically approved by system",
        Guid.Parse(adminUser.Id)
    );
    
    context.MemberApprovals.Add(approval);
    await context.SaveChangesAsync();
    
    Console.WriteLine($"✅ Successfully approved admin user!");
}

// Show final status
var finalApprovals = await context.MemberApprovals
    .Where(a => a.MemberId == member.Id)
    .ToListAsync();

Console.WriteLine($"\n📊 Final Status:");
Console.WriteLine($"  - Member ID: {member.Id}");
Console.WriteLine($"  - Member Name: {member.FullName}");
Console.WriteLine($"  - Email: {member.Email}");
Console.WriteLine($"  - Is Approved: {finalApprovals.Any(a => a.Approved)}");
Console.WriteLine($"  - Total Approvals: {finalApprovals.Count}");

Console.WriteLine("\n=== Done ===");
Console.WriteLine("\n⚠️ IMPORTANT: Please logout and login again for changes to take effect!");
