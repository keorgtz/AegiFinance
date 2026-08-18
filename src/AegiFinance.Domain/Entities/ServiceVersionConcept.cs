namespace AegiFinance.Domain.Entities;

public class ServiceVersionConcept : AuditableEntity
{
    public Guid ServiceVersionId { get; set; }
    public ServiceVersion ServiceVersion { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal TaxPercent { get; set; }
    public int SortOrder { get; set; }
}
