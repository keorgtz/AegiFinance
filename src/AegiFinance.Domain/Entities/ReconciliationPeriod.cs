using AegiFinance.Domain.Enums;

namespace AegiFinance.Domain.Entities;

public sealed class ReconciliationPeriod : AuditableEntity
{
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public Guid BankAccountId { get; set; }
    public BankAccount BankAccount { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Currency { get; set; } = "MXN";
    public decimal BankAmount { get; set; }
    public decimal LedgerAmount { get; set; }
    public decimal DifferenceAmount { get; set; }
    public ReconciliationDifferenceType DifferenceType { get; set; }
    public string? Justification { get; set; }
    public ReconciliationPeriodStatus Status { get; set; } = ReconciliationPeriodStatus.Open;
    public DateTime? ClosedAt { get; set; }
    public Guid? ClosedBy { get; set; }
}
