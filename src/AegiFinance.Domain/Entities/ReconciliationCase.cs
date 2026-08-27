using AegiFinance.Domain.Enums;

namespace AegiFinance.Domain.Entities;

public sealed class ReconciliationCase : AuditableEntity
{
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public Guid BankAccountId { get; set; }
    public BankAccount BankAccount { get; set; } = null!;
    public ReconciliationStatus Status { get; set; } = ReconciliationStatus.Suggested;
    public ReconciliationMatchType MatchType { get; set; }
    public decimal Score { get; set; }
    public string ExplanationJson { get; set; } = "[]";
    public decimal BankAmount { get; set; }
    public decimal LedgerAmount { get; set; }
    public decimal DifferenceAmount { get; set; }
    public ReconciliationDifferenceType DifferenceType { get; set; }
    public string? DifferenceReason { get; set; }
    public bool IsAutomatic { get; set; }
    public DateTime GeneratedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public Guid? ConfirmedBy { get; set; }
    public DateTime? RejectedAt { get; set; }
    public Guid? RejectedBy { get; set; }
    public DateTime? ReversedAt { get; set; }
    public Guid? ReversedBy { get; set; }
    public string? ReversalReason { get; set; }
    public ICollection<ReconciliationCaseBankLine> BankLines { get; set; } = new List<ReconciliationCaseBankLine>();
    public ICollection<ReconciliationCaseLedgerEntry> LedgerEntries { get; set; } = new List<ReconciliationCaseLedgerEntry>();
}
