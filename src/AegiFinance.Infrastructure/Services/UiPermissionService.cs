using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Entities;
using AegiFinance.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Infrastructure.Services;

public sealed class UiPermissionService : IUiPermissionService
{
    private readonly IApplicationDbContext _context;
    private readonly IPermissionService _permissionService;

    public UiPermissionService(IApplicationDbContext context, IPermissionService permissionService)
    {
        _context = context;
        _permissionService = permissionService;
    }

    public async Task<IReadOnlyDictionary<string, string>> GetEffectivePoliciesAsync(
        Guid userId,
        Guid? clientId = null,
        Guid? subscriptionId = null,
        CancellationToken cancellationToken = default)
    {
        var roleIds = await _context.Users.AsNoTracking()
            .Where(user => user.Id == userId)
            .SelectMany(user => user.Roles.Select(role => role.Id))
            .ToListAsync(cancellationToken);
        var permissions = await _permissionService.GetEffectivePermissionsAsync(userId, clientId, subscriptionId, cancellationToken);
        return await ResolveAsync(roleIds, userId, permissions, clientId, subscriptionId, cancellationToken);
    }

    public async Task<IReadOnlyDictionary<string, string>> SimulateAsync(
        Guid roleId,
        Guid? userId,
        Guid? clientId,
        Guid? subscriptionId,
        CancellationToken cancellationToken = default)
    {
        var permissions = await _context.RolePermissions.AsNoTracking()
            .Where(item => item.RoleId == roleId && item.IsGranted && item.Permission.IsActive)
            .Select(item => item.Permission.Code)
            .ToListAsync(cancellationToken);

        if (userId.HasValue)
        {
            var userOverrides = await _context.UserPermissionOverrides.AsNoTracking()
                .Where(item => item.UserId == userId &&
                    (!item.ClientId.HasValue || item.ClientId == clientId) &&
                    (!item.SubscriptionId.HasValue || item.SubscriptionId == subscriptionId) &&
                    (!item.ExpiresAt.HasValue || item.ExpiresAt > DateTime.UtcNow))
                .Select(item => new { item.Permission.Code, item.IsGranted })
                .ToListAsync(cancellationToken);
            foreach (var item in userOverrides)
            {
                if (item.IsGranted) permissions.Add(item.Code); else permissions.RemoveAll(code => code == item.Code);
            }
        }

        return await ResolveAsync([roleId], userId, permissions, clientId, subscriptionId, cancellationToken);
    }

    public async Task SyncCatalogAsync(IReadOnlyCollection<UiControlDefinitionDto> controls, CancellationToken cancellationToken = default)
    {
        var uniqueControls = controls.GroupBy(control => control.ControlKey, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.Last()).ToList();
        var keys = uniqueControls.Select(control => control.ControlKey).ToList();
        var existing = await _context.UiControlDefinitions
            .Where(control => keys.Contains(control.ControlKey))
            .ToDictionaryAsync(control => control.ControlKey, StringComparer.OrdinalIgnoreCase, cancellationToken);

        foreach (var control in uniqueControls)
        {
            if (string.IsNullOrWhiteSpace(control.ControlKey)) continue;
            if (!existing.TryGetValue(control.ControlKey, out var entity))
            {
                entity = new UiControlDefinition { Id = Guid.NewGuid(), ControlKey = control.ControlKey };
                _context.UiControlDefinitions.Add(entity);
            }

            entity.Label = control.Label;
            entity.Module = control.Module;
            entity.ControlType = control.ControlType;
            entity.RequiredPermissionCode = control.RequiredPermissionCode;
            entity.IsSystemRequired = control.IsSystemRequired;
            entity.IsActive = true;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task SetPolicyAsync(UiControlPolicyDto policy, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<UiAccessMode>(policy.AccessMode, true, out var accessMode))
            throw new InvalidOperationException("El modo de acceso de UI no es válido.");
        if (policy.RoleId.HasValue == policy.UserId.HasValue)
            throw new InvalidOperationException("La política debe pertenecer a un rol o a un usuario, pero no a ambos.");

        var definition = await _context.UiControlDefinitions.FirstOrDefaultAsync(item => item.Id == policy.UiControlDefinitionId, cancellationToken)
            ?? throw new InvalidOperationException("El control de UI no existe.");
        if (definition.IsSystemRequired && accessMode != UiAccessMode.Enabled)
            throw new InvalidOperationException("Los controles de seguridad y accesibilidad deben permanecer disponibles.");

        var entity = await _context.UiControlPolicies.FirstOrDefaultAsync(item =>
            item.UiControlDefinitionId == policy.UiControlDefinitionId && item.RoleId == policy.RoleId &&
            item.UserId == policy.UserId && item.ClientId == policy.ClientId && item.SubscriptionId == policy.SubscriptionId,
            cancellationToken);
        if (entity is null)
        {
            entity = new UiControlPolicy { Id = Guid.NewGuid(), UiControlDefinitionId = policy.UiControlDefinitionId };
            _context.UiControlPolicies.Add(entity);
        }

        entity.RoleId = policy.RoleId;
        entity.UserId = policy.UserId;
        entity.ClientId = policy.ClientId;
        entity.SubscriptionId = policy.SubscriptionId;
        entity.AccessMode = accessMode;
        entity.ExpiresAt = policy.ExpiresAt;
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<IReadOnlyDictionary<string, string>> ResolveAsync(
        IReadOnlyCollection<Guid> roleIds,
        Guid? userId,
        IReadOnlyCollection<string> permissions,
        Guid? clientId,
        Guid? subscriptionId,
        CancellationToken cancellationToken)
    {
        var definitions = await _context.UiControlDefinitions.AsNoTracking().Where(item => item.IsActive).ToListAsync(cancellationToken);
        var definitionIds = definitions.Select(item => item.Id).ToList();
        var policies = await _context.UiControlPolicies.AsNoTracking()
            .Where(item => definitionIds.Contains(item.UiControlDefinitionId) &&
                ((item.RoleId.HasValue && roleIds.Contains(item.RoleId.Value)) || (userId.HasValue && item.UserId == userId)) &&
                (!item.ClientId.HasValue || item.ClientId == clientId) &&
                (!item.SubscriptionId.HasValue || item.SubscriptionId == subscriptionId) &&
                (!item.ExpiresAt.HasValue || item.ExpiresAt > DateTime.UtcNow))
            .ToListAsync(cancellationToken);
        var permissionSet = permissions.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var definition in definitions)
        {
            var mode = definition.IsSystemRequired || string.IsNullOrWhiteSpace(definition.RequiredPermissionCode) || permissionSet.Contains(definition.RequiredPermissionCode)
                ? UiAccessMode.Enabled : UiAccessMode.Hidden;
            var rolePolicy = policies.Where(item => item.UiControlDefinitionId == definition.Id && item.RoleId.HasValue)
                .OrderByDescending(item => item.AccessMode).FirstOrDefault();
            if (rolePolicy is not null) mode = rolePolicy.AccessMode;
            var userPolicy = policies.FirstOrDefault(item => item.UiControlDefinitionId == definition.Id && item.UserId == userId);
            if (userPolicy is not null) mode = userPolicy.AccessMode;
            if (definition.IsSystemRequired) mode = UiAccessMode.Enabled;
            result[definition.ControlKey] = mode.ToString();
        }

        return result;
    }
}
