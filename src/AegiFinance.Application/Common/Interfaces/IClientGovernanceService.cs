using AegiFinance.Application.Dtos;

namespace AegiFinance.Application.Common.Interfaces;

public interface IClientGovernanceService
{
    Task<IReadOnlyList<ClientTimelineItemDto>> GetTimelineAsync(Guid clientId, CancellationToken cancellationToken = default);
    Task<ClientDuplicateRuleDto> GetDuplicateRuleAsync(CancellationToken cancellationToken = default);
    Task<ClientDuplicateRuleDto> UpdateDuplicateRuleAsync(ClientDuplicateRuleDto rule, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ClientDuplicateMatchDto>> FindDuplicatesAsync(string name, string? taxId, string? billingEmail, Guid? excludeClientId, CancellationToken cancellationToken = default);
}
