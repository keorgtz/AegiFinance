using AegiFinance.Domain.Enums;

namespace AegiFinance.Domain.Entities;

public class Subscription : AuditableEntity
{
    public string Code { get; set; } = null!;
    public Guid ClientId { get; set; }
    public Client Client { get; set; } = null!;
    public Guid ServiceId { get; set; }
    public Service Service { get; set; } = null!;
    public BillingType BillingType { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "MXN";
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int BillingDay { get; set; }
    public SubscriptionStatus Status { get; set; }
    public bool AutoRenew { get; set; }
    public string? Notes { get; set; }
    public DateTime? LastBillingDate { get; set; }
    public DateTime? NextBillingDate { get; set; }

    public ICollection<SubscriptionPriceHistory> PriceHistory { get; set; } = new List<SubscriptionPriceHistory>();
    public ICollection<SubscriptionChangeLog> ChangeLogs { get; set; } = new List<SubscriptionChangeLog>();
}
