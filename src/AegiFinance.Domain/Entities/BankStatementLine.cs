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
    
    public bool IsReconciled { get; set; }
    
    public Guid? LedgerEntryId { get; set; }
    public LedgerEntry? LedgerEntry { get; set; }
}
