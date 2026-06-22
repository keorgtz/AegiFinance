namespace AegiFinance.Domain.Entities;

public class ServicePriceHistory : AuditableEntity
{
    public Guid ServiceId { get; set; }
    public Service Service { get; set; } = null!;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "MXN";
    public DateTime EffectiveDate { get; set; }
    public string? Reason { get; set; }
}
