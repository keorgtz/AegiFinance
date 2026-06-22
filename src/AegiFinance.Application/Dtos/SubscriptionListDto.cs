namespace AegiFinance.Application.Dtos;

public class SubscriptionListDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string BillingType { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool AutoRenew { get; set; }
    public DateTime? NextBillingDate { get; set; }
}
