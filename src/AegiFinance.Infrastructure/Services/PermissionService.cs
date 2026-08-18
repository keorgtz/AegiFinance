using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Domain.Enums;
using AegiFinance.Domain.Security;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Infrastructure.Services;

public sealed class PermissionService : IPermissionService
{
    private readonly IApplicationDbContext _context;

    public PermissionService(IApplicationDbContext context)
    {
        _context = context;
    }

    public Task<IReadOnlyList<string>> GetEffectivePermissionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        GetEffectivePermissionsAsync(userId, null, null, cancellationToken);

    public async Task<IReadOnlyList<string>> GetEffectivePermissionsAsync(
        Guid userId,
        Guid? clientId,
        Guid? subscriptionId,
        CancellationToken cancellationToken = default)
    {
        var roleGrants = await _context.RolePermissions
            .AsNoTracking()
            .Where(rolePermission =>
                rolePermission.IsGranted &&
                rolePermission.Permission.IsActive &&
                rolePermission.Permission.Kind == PermissionKind.Business &&
                rolePermission.Role.Users.Any(user => user.Id == userId))
            .Select(rolePermission => rolePermission.Permission.Code)
            .ToListAsync(cancellationToken);

        var overrides = await _context.UserPermissionOverrides
            .AsNoTracking()
            .Where(permissionOverride =>
                permissionOverride.UserId == userId &&
                permissionOverride.Permission.IsActive &&
                permissionOverride.Permission.Kind == PermissionKind.Business)
            .Select(permissionOverride => new PermissionOverrideDecision(
                permissionOverride.Permission.Code,
                permissionOverride.IsGranted,
                permissionOverride.ClientId,
                permissionOverride.SubscriptionId,
                permissionOverride.ExpiresAt))
            .ToListAsync(cancellationToken);
        return PermissionDecisionEngine.Resolve(roleGrants, overrides, clientId, subscriptionId, DateTime.UtcNow);
    }

    public async Task<bool> HasPermissionAsync(
        Guid userId,
        string permissionCode,
        CancellationToken cancellationToken = default) =>
        await HasPermissionAsync(userId, permissionCode, null, null, cancellationToken);

    public async Task<bool> HasPermissionAsync(
        Guid userId,
        string permissionCode,
        Guid? clientId,
        Guid? subscriptionId,
        CancellationToken cancellationToken = default)
    {
        var permissions = await GetEffectivePermissionsAsync(userId, clientId, subscriptionId, cancellationToken);
        return permissions.Contains(permissionCode, StringComparer.OrdinalIgnoreCase);
    }
}
