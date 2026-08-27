namespace AegiFinance.Domain.Entities;

public class SubscriptionAllocation : AuditableEntity
{
    public Guid LedgerEntryId { get; set; }
    public LedgerEntry LedgerEntry { get; set; } = null!;
    public Guid BillingItemId { get; set; }
    public BillingItem BillingItem { get; set; } = null!;
    public decimal Amount { get; set; }
    public DateTime AllocatedAt { get; set; }
    public Guid? AllocatedBy { get; set; }
    public bool IsAutomatic { get; set; }
    public Guid? PaymentApplicationId { get; set; }
    public PaymentApplication? PaymentApplication { get; set; }
    public bool IsReversed { get; set; }
    public DateTime? ReversedAt { get; set; }
    public Guid? ReversedBy { get; set; }
    public string? ReversalReason { get; set; }
}
