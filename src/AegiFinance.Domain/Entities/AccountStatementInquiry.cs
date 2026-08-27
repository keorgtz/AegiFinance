using AegiFinance.Domain.Enums;

namespace AegiFinance.Domain.Entities;

public class AccountStatementInquiry : AuditableEntity
{
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public Guid ClientId { get; set; }
    public Client Client { get; set; } = null!;
    public Guid? SubscriptionId { get; set; }
    public Subscription? Subscription { get; set; }
    public Guid? JournalEntryId { get; set; }
    public JournalEntry? JournalEntry { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public string? StatementVerificationCode { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public AccountStatementInquiryStatus Status { get; set; } = AccountStatementInquiryStatus.Open;
    public DateTime RequestedAt { get; set; }
    public Guid? RequestedBy { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public Guid? ResolvedBy { get; set; }
    public string? Resolution { get; set; }
}
