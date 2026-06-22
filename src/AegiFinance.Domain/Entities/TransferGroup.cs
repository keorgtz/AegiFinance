namespace AegiFinance.Domain.Entities;

public class TransferGroup : AuditableEntity
{
    public Guid FromEntryId { get; set; }
    public LedgerEntry FromEntry { get; set; } = null!;
    public Guid ToEntryId { get; set; }
    public LedgerEntry ToEntry { get; set; } = null!;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string? Description { get; set; }
}
