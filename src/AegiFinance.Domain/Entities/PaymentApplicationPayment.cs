namespace AegiFinance.Domain.Entities;

public sealed class PaymentApplicationPayment : AuditableEntity
{
    public Guid PaymentApplicationId { get; set; }
    public PaymentApplication PaymentApplication { get; set; } = null!;
    public Guid LedgerEntryId { get; set; }
    public LedgerEntry LedgerEntry { get; set; } = null!;
    public Guid JournalEntryId { get; set; }
    public JournalEntry JournalEntry { get; set; } = null!;
    public decimal AvailableBefore { get; set; }
    public decimal AppliedAmount { get; set; }
    public decimal UnappliedAfter { get; set; }
}
