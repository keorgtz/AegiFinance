namespace AegiFinance.Application.Dtos;

public record GeneralLedgerAccountDto(
    Guid Id, string Code, string Name, string AccountType, string Purpose,
    string Currency, bool IsSystem, bool IsActive, Guid? ParentAccountId,
    Guid? BankAccountId, decimal Balance);

public record JournalLineDto(
    Guid Id, Guid AccountId, string AccountCode, string AccountName,
    decimal Debit, decimal Credit, string? Description, Guid? ClientId,
    Guid? BankAccountId, Guid? BillingItemId, Guid? LegacyLedgerEntryId);

public record JournalEntryDto(
    Guid Id, string EntryNumber, DateTime Date, string Description, string? Reference,
    string Currency, string Status, string SourceType, string? SourceId,
    Guid AccountingPeriodId, string AccountingPeriodName, Guid? ClientId,
    DateTime? PostedAt, DateTime? ReversedAt, Guid? ReversesJournalEntryId,
    decimal TotalDebit, decimal TotalCredit, IReadOnlyList<JournalLineDto> Lines);

public record AccountingPeriodDto(
    Guid Id, string Name, DateTime StartDate, DateTime EndDate,
    string Status, DateTime? ClosedAt);

public record TrialBalanceLineDto(
    Guid AccountId, string AccountCode, string AccountName, string AccountType,
    decimal Debit, decimal Credit, decimal Balance);

public record TrialBalanceDto(
    DateTime AsOfDate, string Currency, decimal TotalDebit, decimal TotalCredit,
    bool IsBalanced, IReadOnlyList<TrialBalanceLineDto> Lines);

public record AccountLedgerLineDto(
    Guid JournalEntryId, string EntryNumber, DateTime Date, string Description,
    string Status, decimal Debit, decimal Credit, decimal RunningBalance,
    Guid? ClientId, Guid? LegacyLedgerEntryId);

public record LegacyMigrationResultDto(
    int MigratedEntries, int PreviouslyMigratedEntries, decimal LegacySignedTotal,
    decimal JournalBankTotal, decimal Difference, bool IsReconciled);

public record CreateJournalLineRequest(
    Guid AccountId, decimal Debit, decimal Credit, string? Description,
    Guid? ClientId, Guid? BankAccountId, Guid? BillingItemId);

public record CreateJournalEntryRequest(
    DateTime Date, string Description, string? Reference, string Currency,
    Guid? ClientId, string IdempotencyKey, IReadOnlyList<CreateJournalLineRequest> Lines);
