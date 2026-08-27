namespace AegiFinance.Domain.Entities;

public sealed class ReconciliationCaseBankLine : AuditableEntity
{
    public Guid ReconciliationCaseId { get; set; }
    public ReconciliationCase ReconciliationCase { get; set; } = null!;
    public Guid BankStatementLineId { get; set; }
    public BankStatementLine BankStatementLine { get; set; } = null!;
    public decimal AppliedAmount { get; set; }
}
