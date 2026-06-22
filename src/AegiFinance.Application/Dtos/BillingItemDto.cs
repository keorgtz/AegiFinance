namespace AegiFinance.Application.Dtos;

public class BillingItemDto
{
    public Guid Id { get; set; }
    public Guid SubscriptionId { get; set; }
    public string SubscriptionCode { get; set; } = string.Empty;
    public Guid BillingCycleId { get; set; }
    public string BillingCycleLabel { get; set; } = string.Empty;
    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal PaidAmount { get; set; }
    public decimal Balance => Amount - PaidAmount;
    public DateTime GeneratedAt { get; set; }
    public Guid? GeneratedBy { get; set; }
}
