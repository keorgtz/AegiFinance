namespace AegiFinance.Application.Common.Interfaces;

public interface IAllocationService
{
    Task<AllocationResult> AutoAllocateAsync(Guid ledgerEntryId, CancellationToken cancellationToken = default);
    Task<AllocationResult> ManualAllocateAsync(Guid ledgerEntryId, IReadOnlyList<ManualAllocationRequest> allocations, CancellationToken cancellationToken = default);
    Task UnallocateAsync(Guid allocationId, CancellationToken cancellationToken = default);
}

public class AllocationResult
{
    public bool Success { get; set; }
    public decimal AllocatedAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public List<string> Errors { get; set; } = new();
}

public record ManualAllocationRequest(Guid BillingItemId, decimal Amount);
