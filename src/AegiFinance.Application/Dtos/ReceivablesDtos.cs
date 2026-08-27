namespace AegiFinance.Application.Dtos;

public record BillingAdjustmentDto(Guid Id, string Type, decimal Amount, string Reason, DateTime EffectiveDate, DateTime? ReversedAt);
public record PaymentPromiseDto(Guid Id, decimal PromisedAmount, DateTime PromiseDate, string Status, string? Notes, DateTime? ResolvedAt);
public record AgingBucketDto(string Key, string Label, int Count, decimal Amount);
public record ReceivablesAgingDto(DateTime AsOfDate, string Currency, decimal TotalOutstanding, decimal LedgerBalance, decimal Difference, bool IsReconciled, IReadOnlyList<AgingBucketDto> Buckets);
