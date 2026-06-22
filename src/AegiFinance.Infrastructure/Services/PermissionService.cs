using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Infrastructure.Services;

public class PermissionService : IPermissionService
{
    private readonly ApplicationDbContext _context;

    public PermissionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<string>> GetEffectivePermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var rolePermissions = await _context.Roles
            .AsNoTracking()
            .Where(r => r.Users.Any(u => u.Id == userId))
            .SelectMany(r => r.Permissions)
            .Select(p => p.Code)
            .Distinct()
            .ToListAsync(cancellationToken);

        var directPermissions = await _context.UserPermissions
            .AsNoTracking()
            .Where(up => up.UserId == userId)
            .Select(up => new { up.PermissionId, up.IsGranted })
            .ToListAsync(cancellationToken);

        var permissionIds = directPermissions.Select(up => up.PermissionId).ToList();
        var permissionCodes = await _context.Permissions
            .AsNoTracking()
            .Where(p => permissionIds.Contains(p.Id))
            .Select(p => new { p.Id, p.Code })
            .ToDictionaryAsync(p => p.Id, p => p.Code, cancellationToken);

        var effective = new HashSet<string>(rolePermissions);

        foreach (var direct in directPermissions)
        {
            if (!permissionCodes.TryGetValue(direct.PermissionId, out var code))
            {
                continue;
            }

            if (direct.IsGranted)
            {
                effective.Add(code);
            }
            else
            {
                effective.Remove(code);
            }
        }

        return effective.ToList().AsReadOnly();
    }

    public async Task<bool> HasPermissionAsync(Guid userId, string permissionCode, CancellationToken cancellationToken = default)
    {
        var effective = await GetEffectivePermissionsAsync(userId, cancellationToken);
        return effective.Contains(permissionCode);
    }
}
