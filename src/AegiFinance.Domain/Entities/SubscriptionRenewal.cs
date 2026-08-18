namespace AegiFinance.Domain.Entities;

public class SubscriptionRenewal : AuditableEntity
{
    public Guid SubscriptionId { get; set; }
    public Subscription Subscription { get; set; } = null!;
    public string IdempotencyKey { get; set; } = null!;
    public DateTime? PreviousEndDate { get; set; }
    public DateTime? NewEndDate { get; set; }
    public DateTime? PreviousNextBillingDate { get; set; }
    public DateTime? NewNextBillingDate { get; set; }
    public DateTime RenewedAt { get; set; }
    public string? Reason { get; set; }
}
