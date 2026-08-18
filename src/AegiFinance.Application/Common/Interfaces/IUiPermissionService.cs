using AegiFinance.Application.Dtos;

namespace AegiFinance.Application.Common.Interfaces;

public interface IUiPermissionService
{
    Task<IReadOnlyDictionary<string, string>> GetEffectivePoliciesAsync(Guid userId, Guid? clientId = null, Guid? subscriptionId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyDictionary<string, string>> SimulateAsync(Guid roleId, Guid? userId, Guid? clientId, Guid? subscriptionId, CancellationToken cancellationToken = default);
    Task SyncCatalogAsync(IReadOnlyCollection<UiControlDefinitionDto> controls, CancellationToken cancellationToken = default);
    Task SetPolicyAsync(UiControlPolicyDto policy, CancellationToken cancellationToken = default);
}
