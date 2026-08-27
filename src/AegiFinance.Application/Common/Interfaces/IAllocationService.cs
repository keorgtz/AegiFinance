using AegiFinance.Domain.Enums;

namespace AegiFinance.Application.Common.Interfaces;

public interface IAllocationService
{
    Task<AllocationResult> AutoAllocateAsync(Guid ledgerEntryId, CancellationToken cancellationToken = default);
    Task<AllocationResult> ManualAllocateAsync(Guid ledgerEntryId, IReadOnlyList<ManualAllocationRequest> allocations, CancellationToken cancellationToken = default);
    Task UnallocateAsync(Guid allocationId, CancellationToken cancellationToken = default);
    Task<AllocationResult> ApplyAutomaticallyAsync(AutoPaymentApplicationRequest request, CancellationToken cancellationToken = default);
    Task<AllocationResult> ApplyManuallyAsync(ManualPaymentApplicationRequest request, CancellationToken cancellationToken = default);
    Task ReverseApplicationAsync(Guid applicationId, string reason, CancellationToken cancellationToken = default);
    Task<AllocationResult> ReapplyAsync(Guid applicationId, PaymentApplicationPriority? priority, Guid? preferredServiceId, string idempotencyKey, CancellationToken cancellationToken = default);
}

public class AllocationResult
{
    public bool Success { get; set; }
    public decimal AllocatedAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public Guid? PaymentApplicationId { get; set; }
    public string? ReceiptNumber { get; set; }
    public List<string> Errors { get; set; } = new();
}

public record ManualAllocationRequest(Guid BillingItemId, decimal Amount);
public record PaymentAllocationLineRequest(Guid LedgerEntryId, Guid BillingItemId, decimal Amount);
public record AutoPaymentApplicationRequest(IReadOnlyList<Guid> LedgerEntryIds, PaymentApplicationPriority? Priority, Guid? PreferredServiceId, string IdempotencyKey, Guid? ReappliesPaymentApplicationId = null);
public record ManualPaymentApplicationRequest(IReadOnlyList<PaymentAllocationLineRequest> Allocations, string IdempotencyKey);
