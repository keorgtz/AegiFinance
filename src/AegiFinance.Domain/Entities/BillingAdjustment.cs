using AegiFinance.Domain.Enums;

namespace AegiFinance.Domain.Entities;

public class BillingAdjustment : AuditableEntity
{
    public Guid BillingItemId { get; set; }
    public BillingItem BillingItem { get; set; } = null!;
    public BillingAdjustmentType Type { get; set; }
    public decimal Amount { get; set; }
    public string Reason { get; set; } = null!;
    public DateTime EffectiveDate { get; set; }
    public string IdempotencyKey { get; set; } = null!;
    public DateTime? ReversedAt { get; set; }
    public Guid? ReversedBy { get; set; }
    public string? ReversalReason { get; set; }
}
