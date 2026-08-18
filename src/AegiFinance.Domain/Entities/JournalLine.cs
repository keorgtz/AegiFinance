namespace AegiFinance.Domain.Entities;

public class JournalLine : AuditableEntity
{
    public Guid JournalEntryId { get; set; }
    public JournalEntry JournalEntry { get; set; } = null!;
    public Guid AccountId { get; set; }
    public GeneralLedgerAccount Account { get; set; } = null!;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string? Description { get; set; }
    public Guid? ClientId { get; set; }
    public Client? Client { get; set; }
    public Guid? BankAccountId { get; set; }
    public BankAccount? BankAccount { get; set; }
    public Guid? BillingItemId { get; set; }
    public BillingItem? BillingItem { get; set; }
    public Guid? LegacyLedgerEntryId { get; set; }
    public LedgerEntry? LegacyLedgerEntry { get; set; }
}
