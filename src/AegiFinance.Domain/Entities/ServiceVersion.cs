using AegiFinance.Domain.Enums;

namespace AegiFinance.Domain.Entities;

public class ServiceVersion : AuditableEntity
{
    public Guid ServiceId { get; set; }
    public Service Service { get; set; } = null!;
    public int VersionNumber { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public BillingType BillingType { get; set; }
    public decimal BasePrice { get; set; }
    public string Currency { get; set; } = "MXN";
    public decimal DefaultDiscountPercent { get; set; }
    public decimal DefaultTaxPercent { get; set; }
    public int? CustomIntervalDays { get; set; }
    public ProrationPolicy ProrationPolicy { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public string? Terms { get; set; }
    public bool IsPublished { get; set; }
    public ICollection<ServiceVersionConcept> Concepts { get; set; } = new List<ServiceVersionConcept>();
}
