using AegiFinance.Domain.Enums;

namespace AegiFinance.Domain.Entities;

public class SubscriptionChangeLog : AuditableEntity
{
    public Guid SubscriptionId { get; set; }
    public Subscription Subscription { get; set; } = null!;
    public SubscriptionChangeType ChangeType { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? Reason { get; set; }
}
