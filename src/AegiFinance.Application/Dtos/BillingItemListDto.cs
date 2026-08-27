namespace AegiFinance.Application.Dtos;

public class BillingItemListDto
{
    public Guid Id { get; set; }
    public string SubscriptionCode { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal BaseAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ProrationFactor { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal PaidAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public decimal LateFeeAmount { get; set; }
    public decimal Balance => Amount + LateFeeAmount - CreditAmount - PaidAmount;
    public PaymentPromiseDto? ActivePromise { get; set; }
    public string? CancellationReason { get; set; }
}
