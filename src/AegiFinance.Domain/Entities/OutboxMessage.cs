using AegiFinance.Domain.Enums;

namespace AegiFinance.Domain.Entities;

public sealed class OutboxMessage : AuditableEntity
{
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public string EventType { get; set; } = string.Empty;
    public string IdempotencyKey { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = "{}";
    public OutboxMessageStatus Status { get; set; } = OutboxMessageStatus.Pending;
    public DateTime AvailableAt { get; set; }
    public DateTime? LockedUntil { get; set; }
    public string? LockOwner { get; set; }
    public int AttemptCount { get; set; }
    public int MaxAttempts { get; set; } = 5;
    public DateTime? ProcessedAt { get; set; }
    public string? LastError { get; set; }
    public string? TraceId { get; set; }
}
