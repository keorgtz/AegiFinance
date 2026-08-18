using AegiFinance.Domain.Enums;

namespace AegiFinance.Domain.Entities;

public class SubscriptionTermsVersion : AuditableEntity
{
    public Guid SubscriptionId { get; set; }
    public Subscription Subscription { get; set; } = null!;
    public int VersionNumber { get; set; }
    public Guid? ServiceVersionId { get; set; }
    public ServiceVersion? ServiceVersion { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public BillingType BillingType { get; set; }
    public decimal BasePrice { get; set; }
    public string Currency { get; set; } = "MXN";
    public decimal DiscountPercent { get; set; }
    public decimal TaxPercent { get; set; }
    public int BillingDay { get; set; }
    public int? CustomIntervalDays { get; set; }
    public ProrationPolicy ProrationPolicy { get; set; }
    public string? Terms { get; set; }
    public string? Reason { get; set; }
}
