using AegiFinance.Domain.Enums;

namespace AegiFinance.Application.Features.Allocations;

public sealed class PaymentApplicationDto
{
    public Guid Id { get; init; }
    public Guid ClientId { get; init; }
    public string ClientName { get; init; } = string.Empty;
    public string ReceiptNumber { get; init; } = string.Empty;
    public PaymentApplicationPriority Priority { get; init; }
    public PaymentApplicationOrigin Origin { get; init; }
    public PaymentApplicationStatus Status { get; init; }
    public decimal TotalPaymentAmount { get; init; }
    public decimal AppliedAmount { get; init; }
    public decimal UnappliedAmount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public DateTime AppliedAt { get; init; }
    public Guid? AppliedBy { get; init; }
    public DateTime? ReversedAt { get; init; }
    public string? ReversalReason { get; init; }
    public Guid? ReappliesPaymentApplicationId { get; init; }
    public IReadOnlyList<PaymentApplicationPaymentDto> Payments { get; init; } = [];
    public IReadOnlyList<PaymentApplicationLineDto> Allocations { get; init; } = [];
}

public sealed class PaymentApplicationPaymentDto
{
    public Guid LedgerEntryId { get; init; }
    public string Description { get; init; } = string.Empty;
    public string? Reference { get; init; }
    public DateTime Date { get; init; }
    public Guid JournalEntryId { get; init; }
    public string JournalEntryNumber { get; init; } = string.Empty;
    public decimal AvailableBefore { get; init; }
    public decimal AppliedAmount { get; init; }
    public decimal UnappliedAfter { get; init; }
}

public sealed class PaymentApplicationLineDto
{
    public Guid Id { get; init; }
    public Guid LedgerEntryId { get; init; }
    public Guid BillingItemId { get; init; }
    public string BillingItemDescription { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public bool IsReversed { get; init; }
}

public sealed class PaymentApplicationSettingsDto
{
    public PaymentApplicationPriority DefaultPriority { get; init; }
}
