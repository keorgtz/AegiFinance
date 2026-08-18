namespace AegiFinance.Application.Dtos;

public class SubscriptionDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public Guid ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string BillingType { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int BillingDay { get; set; }
    public Guid? ServiceVersionId { get; set; }
    public int? CustomIntervalDays { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal TaxPercent { get; set; }
    public string ProrationPolicy { get; set; } = "None";
    public string? ContractTerms { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool AutoRenew { get; set; }
    public string? Notes { get; set; }
    public DateTime? LastBillingDate { get; set; }
    public DateTime? NextBillingDate { get; set; }
}
