namespace AegiFinance.Application.Dtos;

public class BillingItemListDto
{
    public Guid Id { get; set; }
    public string SubscriptionCode { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal PaidAmount { get; set; }
    public decimal Balance => Amount - PaidAmount;
}
