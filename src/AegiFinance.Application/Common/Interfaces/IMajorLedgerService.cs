using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Entities;

namespace AegiFinance.Application.Common.Interfaces;

public interface IMajorLedgerService
{
    Task EnsureBaseChartAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GeneralLedgerAccountDto>> GetAccountsAsync(DateTime? asOfDate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AccountingPeriodDto>> GetPeriodsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<JournalEntryDto>> GetJournalEntriesAsync(DateTime? from, DateTime? to, string? status, CancellationToken cancellationToken = default);
    Task<TrialBalanceDto> GetTrialBalanceAsync(DateTime asOfDate, string currency, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AccountLedgerLineDto>> GetAccountLedgerAsync(Guid accountId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
    Task<JournalEntryDto> CreateDraftAsync(CreateJournalEntryRequest request, CancellationToken cancellationToken = default);
    Task<JournalEntryDto> PostAsync(Guid journalEntryId, CancellationToken cancellationToken = default);
    Task<JournalEntryDto> ReverseAsync(Guid journalEntryId, DateTime reversalDate, string reason, CancellationToken cancellationToken = default);
    Task<AccountingPeriodDto> CreatePeriodAsync(string name, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<AccountingPeriodDto> ClosePeriodAsync(Guid periodId, CancellationToken cancellationToken = default);
    Task<LegacyMigrationResultDto> MigrateLegacyAsync(CancellationToken cancellationToken = default);
    Task<JournalEntry> PostLegacyEntryAsync(LedgerEntry ledgerEntry, CancellationToken cancellationToken = default);
    Task<JournalEntry> PostTransferAsync(LedgerEntry fromEntry, LedgerEntry toEntry, CancellationToken cancellationToken = default);
    Task<JournalEntry> PostChargeAsync(BillingItem billingItem, CancellationToken cancellationToken = default);
    Task<JournalEntry> PostBillingAdjustmentAsync(BillingAdjustment adjustment, CancellationToken cancellationToken = default);
    Task<JournalEntry?> PostOpeningBalanceAsync(BankAccount bankAccount, CancellationToken cancellationToken = default);
}
