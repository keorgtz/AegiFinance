using AegiFinance.Domain.Enums;

namespace AegiFinance.Application.Features.Reconciliation;

public sealed class ReconciliationSettingsDto
{
    public int DateToleranceDays { get; init; }
    public decimal AmountTolerance { get; init; }
    public decimal SuggestionThreshold { get; init; }
    public decimal AutoConfirmThreshold { get; init; }
    public bool AllowAutoConfirmExact { get; init; }
    public int AmountWeight { get; init; }
    public int DateWeight { get; init; }
    public int ReferenceWeight { get; init; }
    public int ClientWeight { get; init; }
    public int PatternWeight { get; init; }
}

public sealed class ReconciliationFactorDto
{
    public string Code { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public decimal Points { get; init; }
    public string Detail { get; init; } = string.Empty;
}

public sealed class ReconciliationBankLineDto
{
    public Guid Id { get; init; }
    public DateTime Date { get; init; }
    public string Description { get; init; } = string.Empty;
    public string? Reference { get; init; }
    public decimal Amount { get; init; }
    public decimal AppliedAmount { get; init; }
    public string Currency { get; init; } = string.Empty;
}

public sealed class ReconciliationLedgerEntryDto
{
    public Guid Id { get; init; }
    public DateTime Date { get; init; }
    public string Description { get; init; } = string.Empty;
    public string? Reference { get; init; }
    public string? ClientName { get; init; }
    public decimal Amount { get; init; }
    public decimal AppliedAmount { get; init; }
    public string Currency { get; init; } = string.Empty;
}

public sealed class ReconciliationCaseDto
{
    public Guid Id { get; init; }
    public Guid BankAccountId { get; init; }
    public string BankAccountName { get; init; } = string.Empty;
    public ReconciliationStatus Status { get; init; }
    public ReconciliationMatchType MatchType { get; init; }
    public decimal Score { get; init; }
    public decimal BankAmount { get; init; }
    public decimal LedgerAmount { get; init; }
    public decimal DifferenceAmount { get; init; }
    public ReconciliationDifferenceType DifferenceType { get; init; }
    public string? DifferenceReason { get; init; }
    public bool IsAutomatic { get; init; }
    public DateTime GeneratedAt { get; init; }
    public DateTime? ConfirmedAt { get; init; }
    public DateTime? ReversedAt { get; init; }
    public IReadOnlyList<ReconciliationFactorDto> Factors { get; init; } = [];
    public IReadOnlyList<ReconciliationBankLineDto> BankLines { get; init; } = [];
    public IReadOnlyList<ReconciliationLedgerEntryDto> LedgerEntries { get; init; } = [];
}

public sealed class ReconciliationRunResultDto
{
    public int SuggestionsCreated { get; init; }
    public int ExactMatches { get; init; }
    public int CombinedMatches { get; init; }
    public int PartialMatches { get; init; }
    public int AutoConfirmed { get; init; }
}

public sealed class ReconciliationSelection
{
    public Guid Id { get; set; }
    public decimal AppliedAmount { get; set; }
}

public sealed class ReconciliationPeriodDto
{
    public Guid Id { get; init; }
    public Guid BankAccountId { get; init; }
    public string BankAccountName { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public string Currency { get; init; } = string.Empty;
    public decimal BankAmount { get; init; }
    public decimal LedgerAmount { get; init; }
    public decimal DifferenceAmount { get; init; }
    public ReconciliationDifferenceType DifferenceType { get; init; }
    public string? Justification { get; init; }
    public ReconciliationPeriodStatus Status { get; init; }
    public DateTime? ClosedAt { get; init; }
}
