using AegiFinance.Domain.Enums;

namespace AegiFinance.Domain.Entities;

public class BillingItem : AuditableEntity
{
    public Guid SubscriptionId { get; set; }
    public Subscription Subscription { get; set; } = null!;
    public Guid BillingCycleId { get; set; }
    public BillingCycle BillingCycle { get; set; } = null!;
    public Guid ClientId { get; set; }
    public Client Client { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "MXN";
    public DateTime DueDate { get; set; }
    public BillingItemStatus Status { get; set; }
    public decimal PaidAmount { get; set; }
    public DateTime GeneratedAt { get; set; }
    public Guid? GeneratedBy { get; set; }
    public string? CancellationReason { get; set; }
}
