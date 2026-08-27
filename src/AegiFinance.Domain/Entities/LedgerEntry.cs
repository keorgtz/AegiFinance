using AegiFinance.Domain.Enums;

namespace AegiFinance.Domain.Entities;

public class LedgerEntry : AuditableEntity
{
    public Guid BankAccountId { get; set; }
    public BankAccount BankAccount { get; set; } = null!;
    public LedgerEntryType EntryType { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "MXN";
    public DateTime Date { get; set; }
    public string Description { get; set; } = null!;
    public string? Reference { get; set; }
    public Guid? ClientId { get; set; }
    public Client? Client { get; set; }
    public Guid? BillingItemId { get; set; }
    public BillingItem? BillingItem { get; set; }
    public bool IsReconciled { get; set; }
    public DateTime? ReconciledAt { get; set; }
    public int ReconciliationVersion { get; set; }
}
