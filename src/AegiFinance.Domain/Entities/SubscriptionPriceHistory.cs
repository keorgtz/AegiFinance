namespace AegiFinance.Domain.Entities;

public class SubscriptionPriceHistory : AuditableEntity
{
    public Guid SubscriptionId { get; set; }
    public Subscription Subscription { get; set; } = null!;
    public decimal OldPrice { get; set; }
    public decimal NewPrice { get; set; }
    public DateTime EffectiveDate { get; set; }
    public string? Reason { get; set; }
}
