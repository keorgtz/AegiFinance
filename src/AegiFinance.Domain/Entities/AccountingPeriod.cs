using AegiFinance.Domain.Enums;

namespace AegiFinance.Domain.Entities;

public class AccountingPeriod : AuditableEntity
{
    public string Name { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public AccountingPeriodStatus Status { get; set; } = AccountingPeriodStatus.Open;
    public DateTime? ClosedAt { get; set; }
    public Guid? ClosedBy { get; set; }
}
