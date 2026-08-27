namespace AegiFinance.Application.Dtos;

public class SubscriptionAllocationDto
{
    public Guid Id { get; set; }
    public Guid LedgerEntryId { get; set; }
    public Guid BillingItemId { get; set; }
    public decimal Amount { get; set; }
    public DateTime AllocatedAt { get; set; }
    public Guid? AllocatedBy { get; set; }
    public bool IsAutomatic { get; set; }
    public bool IsReversed { get; set; }
    public Guid? PaymentApplicationId { get; set; }
    public DateTime? ReversedAt { get; set; }
    public string? ReversalReason { get; set; }
    public string? BillingItemDescription { get; set; }
    public string? LedgerEntryDescription { get; set; }
}
