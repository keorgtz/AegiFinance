namespace AegiFinance.Application.Common.Interfaces;

public interface IPermissionService
{
    Task<IReadOnlyList<string>> GetEffectivePermissionsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> GetEffectivePermissionsAsync(Guid userId, Guid? clientId, Guid? subscriptionId, CancellationToken cancellationToken = default);
    Task<bool> HasPermissionAsync(Guid userId, string permissionCode, CancellationToken cancellationToken = default);
    Task<bool> HasPermissionAsync(Guid userId, string permissionCode, Guid? clientId, Guid? subscriptionId, CancellationToken cancellationToken = default);
}
