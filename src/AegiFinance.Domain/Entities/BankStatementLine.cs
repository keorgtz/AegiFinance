namespace AegiFinance.Domain.Entities;

public class BankStatementLine : AuditableEntity
{
    public Guid BankStatementId { get; set; }
    public BankStatement BankStatement { get; set; } = null!;
    public DateTime TransactionDate { get; set; }
    public string Description { get; set; } = null!;
    public string? Reference { get; set; }
    
    // Positive = Income, Negative = Expense
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "MXN";
    public decimal? BankBalance { get; set; }
    public string? DeduplicationHash { get; set; }
    public int? SourceRowNumber { get; set; }
    
    public bool IsReconciled { get; set; }
    public int ReconciliationVersion { get; set; }
    
    public Guid? LedgerEntryId { get; set; }
    public LedgerEntry? LedgerEntry { get; set; }
}
