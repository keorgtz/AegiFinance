using AegiFinance.Application.Dtos;

namespace AegiFinance.Application.Common.Interfaces;

public interface IAccountingGovernanceService
{
    Task<IReadOnlyList<AccountingPeriodDto>> GetPeriodsAsync(CancellationToken cancellationToken = default);
    Task<AccountingPeriodChecklistDto> GetChecklistAsync(Guid periodId, CancellationToken cancellationToken = default);
    Task<AccountingPeriodDto> CloseAsync(Guid periodId, string verificationCode, CancellationToken cancellationToken = default);
    Task<AccountingPeriodReopenRequestDto> RequestReopenAsync(Guid periodId, string reason, CancellationToken cancellationToken = default);
    Task<AccountingPeriodReopenRequestDto> ReviewReopenAsync(Guid requestId, bool approve, string comment, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AccountingPeriodReopenRequestDto>> GetReopenRequestsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AccountingIntegrityAlertDto>> GetIntegrityAlertsAsync(CancellationToken cancellationToken = default);
    Task<AccountingEvidenceSummaryDto> GetEvidenceAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
}
