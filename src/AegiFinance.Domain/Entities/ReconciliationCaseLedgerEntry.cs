namespace AegiFinance.Domain.Entities;

public sealed class ReconciliationCaseLedgerEntry : AuditableEntity
{
    public Guid ReconciliationCaseId { get; set; }
    public ReconciliationCase ReconciliationCase { get; set; } = null!;
    public Guid LedgerEntryId { get; set; }
    public LedgerEntry LedgerEntry { get; set; } = null!;
    public decimal AppliedAmount { get; set; }
}
