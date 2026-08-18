using AegiFinance.Domain.Enums;

namespace AegiFinance.Domain.Entities;

public class Subscription : AuditableEntity
{
    public string Code { get; set; } = null!;
    public Guid ClientId { get; set; }
    public Client Client { get; set; } = null!;
    public Guid ServiceId { get; set; }
    public Service Service { get; set; } = null!;
    public Guid? ServiceVersionId { get; set; }
    public ServiceVersion? ServiceVersion { get; set; }
    public BillingType BillingType { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "MXN";
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int BillingDay { get; set; }
    public int? CustomIntervalDays { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal TaxPercent { get; set; }
    public ProrationPolicy ProrationPolicy { get; set; }
    public string? ContractTerms { get; set; }
    public SubscriptionStatus Status { get; set; }
    public bool AutoRenew { get; set; }
    public string? Notes { get; set; }
    public DateTime? LastBillingDate { get; set; }
    public DateTime? NextBillingDate { get; set; }

    public ICollection<SubscriptionPriceHistory> PriceHistory { get; set; } = new List<SubscriptionPriceHistory>();
    public ICollection<SubscriptionChangeLog> ChangeLogs { get; set; } = new List<SubscriptionChangeLog>();
    public ICollection<SubscriptionTermsVersion> TermsVersions { get; set; } = new List<SubscriptionTermsVersion>();
    public ICollection<SubscriptionRenewal> Renewals { get; set; } = new List<SubscriptionRenewal>();
}
