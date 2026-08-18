using AegiFinance.Domain.Enums;

namespace AegiFinance.Domain.Entities;

public class Service : AuditableEntity
{
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public Guid? CategoryId { get; set; }
    public ServiceCategory? Category { get; set; }
    public BillingType BillingType { get; set; }
    public decimal DefaultPrice { get; set; }
    public string Currency { get; set; } = "MXN";
    public bool IsActive { get; set; }
    public bool IsPublic { get; set; }
    public ICollection<ServicePriceHistory> PriceHistory { get; set; } = new List<ServicePriceHistory>();
    public ICollection<ServiceVersion> Versions { get; set; } = new List<ServiceVersion>();
}
