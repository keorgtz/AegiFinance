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
    public Guid? SubscriptionTermsVersionId { get; set; }
    public SubscriptionTermsVersion? SubscriptionTermsVersion { get; set; }
    public decimal BaseAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ProrationFactor { get; set; } = 1m;
    public string Currency { get; set; } = "MXN";
    public DateTime DueDate { get; set; }
    public BillingItemStatus Status { get; set; }
    public decimal PaidAmount { get; set; }
    public DateTime GeneratedAt { get; set; }
    public Guid? GeneratedBy { get; set; }
    public string? CancellationReason { get; set; }
}
