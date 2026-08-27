using AegiFinance.Domain.Enums;

namespace AegiFinance.Domain.Entities;

public class AccountingPeriod : AuditableEntity
{
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public string Name { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public AccountingPeriodStatus Status { get; set; } = AccountingPeriodStatus.Open;
    public DateTime? ClosedAt { get; set; }
    public Guid? ClosedBy { get; set; }
    public string? CloseChecklistJson { get; set; }
    public string? CloseVerificationCode { get; set; }
    public DateTime? ReopenedAt { get; set; }
    public Guid? ReopenedBy { get; set; }
    public string? ReopenReason { get; set; }
    public int GovernanceVersion { get; set; }
    public ICollection<AccountingPeriodReopenRequest> ReopenRequests { get; set; } = new List<AccountingPeriodReopenRequest>();
}
