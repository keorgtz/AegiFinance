using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Domain.Entities;
using AegiFinance.Domain.Enums;
using AegiFinance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Web.Seed;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = serviceProvider.GetRequiredService<IPasswordHasher>();

        await context.Database.MigrateAsync();

        // Seed Currency MXN
        if (!await context.CurrencyConfigs.AnyAsync(c => c.Code == "MXN"))
        {
            context.CurrencyConfigs.Add(new CurrencyConfig
            {
                Id = Guid.NewGuid(),
                Code = "MXN",
                Name = "Peso Mexicano",
                Symbol = "$",
                IsActive = true,
                IsDefault = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        // Seed Permissions
        var permissionCodes = new[]
        {
            "ViewDashboard", "ViewReports", "ViewRevenue", "ViewPayments",
            "ViewSubscriptions", "ViewLicenses", "ViewTickets", "ViewClients",
            "ManageUsers", "ManageBilling", "ManageReconciliation", "ManageRoles"
        };

        foreach (var code in permissionCodes)
        {
            if (!await context.Permissions.AnyAsync(p => p.Code == code))
            {
                context.Permissions.Add(new Permission
                {
                    Id = Guid.NewGuid(),
                    Code = code,
                    Name = code,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        await context.SaveChangesAsync();

        // Seed Admin Role
        var adminRole = await context.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Name == "Admin");
        if (adminRole == null)
        {
            adminRole = new Role
            {
                Id = Guid.NewGuid(),
                Name = "Admin",
                Description = "Administrador del sistema",
                UserType = UserType.Administrator,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Roles.Add(adminRole);
            await context.SaveChangesAsync();
        }

        // Assign all permissions to Admin role
        var allPermissions = await context.Permissions.ToListAsync();
        foreach (var perm in allPermissions)
        {
            if (!adminRole.Permissions.Any(p => p.Id == perm.Id))
            {
                adminRole.Permissions.Add(perm);
            }
        }

        // Seed default admin user
        if (!await context.Users.AnyAsync(u => u.UserName == "Admin"))
        {
            var adminUser = new User
            {
                Id = Guid.NewGuid(),
                UserName = "Admin",
                Email = "admin@aegifinance.com",
                PasswordHash = passwordHasher.HashPassword("Admin"),
                UserType = UserType.Administrator,
                Name = "Administrador",
                IsActive = true,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            adminUser.Roles.Add(adminRole);
            context.Users.Add(adminUser);
        }

        await context.SaveChangesAsync();
    }
}
