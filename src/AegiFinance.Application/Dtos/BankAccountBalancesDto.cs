namespace AegiFinance.Application.Dtos;

public sealed record BankAccountBalancesDto(
    Guid BankAccountId,
    string Currency,
    DateTime AsOfDate,
    decimal LedgerBalance,
    decimal? BankBalance,
    DateTime? BankBalanceAsOfDate,
    decimal? ComparisonLedgerBalance,
    decimal? Difference);
