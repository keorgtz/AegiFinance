using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Domain.Entities;
using AegiFinance.Domain.Enums;
using AegiFinance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace AegiFinance.Web.Seed;

public static class SeedData
{
    public static readonly Guid DefaultOrganizationId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static async Task InitializeAsync(
        IServiceProvider serviceProvider,
        IEnumerable<string>? discoveredPermissionCodes = null)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = serviceProvider.GetRequiredService<IPasswordHasher>();
        var majorLedger = serviceProvider.GetRequiredService<IMajorLedgerService>();

        var organization = await context.Organizations.IgnoreQueryFilters().FirstOrDefaultAsync(item => item.Id == DefaultOrganizationId);
        if (organization is null)
        {
            organization = new Organization { Id = DefaultOrganizationId, Code = "DEFAULT", Name = "AegiFinance", IsActive = true };
            context.Organizations.Add(organization);
            await context.SaveChangesAsync();
        }
        await context.Users.IgnoreQueryFilters().Where(item => item.OrganizationId == null).ExecuteUpdateAsync(setters => setters.SetProperty(item => item.OrganizationId, DefaultOrganizationId));
        await context.Clients.IgnoreQueryFilters().Where(item => item.OrganizationId == Guid.Empty).ExecuteUpdateAsync(setters => setters.SetProperty(item => item.OrganizationId, DefaultOrganizationId));
        await context.BankAccounts.IgnoreQueryFilters().Where(item => item.OrganizationId == Guid.Empty).ExecuteUpdateAsync(setters => setters.SetProperty(item => item.OrganizationId, DefaultOrganizationId));
        if (!await context.ClientDuplicateRules.IgnoreQueryFilters().AnyAsync(item => item.OrganizationId == DefaultOrganizationId))
            context.ClientDuplicateRules.Add(new ClientDuplicateRule { Id = Guid.NewGuid(), OrganizationId = DefaultOrganizationId });

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
        var permissionCodes = new HashSet<string>(StringComparer.Ordinal)
        {
            "ViewDashboard", "ViewReports", "ViewRevenue", "ViewPayments",
            "ViewSubscriptions", "ViewLicenses", "ViewTickets", "ViewClients",
            "ViewServices", "ManageClients", "ManageServices", "ManageSubscriptions",
            "ManageUsers", "ManageBilling", "ManageReconciliation", "ManageRoles"
        };

        if (discoveredPermissionCodes is not null)
        {
            permissionCodes.UnionWith(discoveredPermissionCodes.Where(code => !string.IsNullOrWhiteSpace(code)));
        }

        foreach (var code in permissionCodes)
        {
            var (module, action) = DescribePermission(code);
            var permission = await context.Permissions.FirstOrDefaultAsync(p => p.Code == code);
            if (permission is null)
            {
                context.Permissions.Add(new PermissionDefinition
                {
                    Id = Guid.NewGuid(),
                    Code = code,
                    Name = Regex.Replace(code, "([a-z0-9])([A-Z])", "$1 $2"),
                    Module = module,
                    Action = action,
                    Kind = PermissionKind.Business,
                    IsSystemGenerated = discoveredPermissionCodes?.Contains(code) == true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
            else
            {
                permission.Module = module;
                permission.Action = action;
                permission.Kind = PermissionKind.Business;
                permission.IsActive = true;
            }
        }

        await context.SaveChangesAsync();
        await majorLedger.EnsureBaseChartAsync();

        // Seed Admin Role
        var adminRole = await context.Roles
            .Include(r => r.RolePermissions)
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
            if (!adminRole.RolePermissions.Any(rolePermission => rolePermission.PermissionId == perm.Id))
            {
                adminRole.RolePermissions.Add(new RolePermission
                {
                    RoleId = adminRole.Id,
                    PermissionId = perm.Id,
                    IsGranted = true
                });
            }
        }

        // Seed default admin user
        if (!await context.Users.AnyAsync(u => u.UserName == "Admin"))
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var environment = serviceProvider.GetRequiredService<IHostEnvironment>();
            var initialAdminPassword = configuration["AdminSeed:Password"];
            if (string.IsNullOrWhiteSpace(initialAdminPassword))
            {
                if (!environment.IsDevelopment())
                    throw new InvalidOperationException("AdminSeed:Password debe configurarse para crear el administrador inicial en producción.");
                initialAdminPassword = "Admin";
            }
            if (!environment.IsDevelopment() && initialAdminPassword.Length < 12)
                throw new InvalidOperationException("AdminSeed:Password debe tener al menos 12 caracteres en producción.");

            var adminUser = new User
            {
                Id = Guid.NewGuid(),
                UserName = "Admin",
                Email = "admin@aegifinance.com",
                PasswordHash = passwordHasher.HashPassword(initialAdminPassword),
                UserType = UserType.Administrator,
                OrganizationId = DefaultOrganizationId,
                Name = "Administrador",
                IsActive = true,
                EmailConfirmed = true,
                MustChangePassword = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            adminUser.Roles.Add(adminRole);
            context.Users.Add(adminUser);
        }

        await context.SaveChangesAsync();
    }

    private static (string Module, string Action) DescribePermission(string code)
    {
        var action = Regex.Match(code, "^(View|Create|Update|Delete|Manage|Generate|Cancel|Close|Reconcile|Reprocess|Allocate|Unallocate|Sync)").Value;
        if (string.IsNullOrWhiteSpace(action))
        {
            action = "Execute";
        }

        var module = code[action.Length..];
        return (string.IsNullOrWhiteSpace(module) ? "System" : module, action);
    }
}
