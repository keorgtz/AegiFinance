using AegiFinance.Domain.Enums;

namespace AegiFinance.Domain.Entities;

public sealed class AccountingPeriodReopenRequest : AuditableEntity
{
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public Guid AccountingPeriodId { get; set; }
    public AccountingPeriod AccountingPeriod { get; set; } = null!;
    public string Reason { get; set; } = string.Empty;
    public AccountingPeriodReopenStatus Status { get; set; } = AccountingPeriodReopenStatus.Pending;
    public DateTime RequestedAt { get; set; }
    public Guid RequestedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public Guid? ReviewedBy { get; set; }
    public string? ReviewComment { get; set; }
}
