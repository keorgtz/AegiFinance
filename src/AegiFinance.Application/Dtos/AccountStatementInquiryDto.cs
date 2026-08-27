namespace AegiFinance.Application.Dtos;

public sealed class AccountStatementInquiryDto
{
    public Guid Id { get; init; }
    public Guid ClientId { get; init; }
    public Guid? SubscriptionId { get; init; }
    public string? SubscriptionCode { get; init; }
    public Guid? JournalEntryId { get; init; }
    public string? JournalEntryNumber { get; init; }
    public string? StatementVerificationCode { get; init; }
    public string Subject { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime RequestedAt { get; init; }
    public DateTime? ResolvedAt { get; init; }
    public string? Resolution { get; init; }
}
