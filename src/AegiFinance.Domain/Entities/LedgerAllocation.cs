namespace AegiFinance.Domain.Entities;

public class LedgerAllocation : AuditableEntity
{
    public Guid LedgerEntryId { get; set; }
    public LedgerEntry LedgerEntry { get; set; } = null!;
    public Guid BillingItemId { get; set; }
    public BillingItem BillingItem { get; set; } = null!;
    public decimal Amount { get; set; }
}
