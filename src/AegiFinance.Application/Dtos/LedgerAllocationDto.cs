namespace AegiFinance.Application.Dtos;

public class LedgerAllocationDto
{
    public Guid Id { get; set; }
    public Guid LedgerEntryId { get; set; }
    public Guid BillingItemId { get; set; }
    public string BillingItemDescription { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
