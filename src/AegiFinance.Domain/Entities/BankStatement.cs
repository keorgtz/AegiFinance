namespace AegiFinance.Domain.Entities;

public class BankStatement : AuditableEntity
{
    public Guid BankAccountId { get; set; }
    public BankAccount BankAccount { get; set; } = null!;
    public DateTime StatementDate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal ClosingBalance { get; set; }
    public bool IsBalanceVerified { get; set; }
    public string? FileUrl { get; set; }
    
    // Navigation property
    public ICollection<BankStatementLine> Lines { get; set; } = new List<BankStatementLine>();
}
