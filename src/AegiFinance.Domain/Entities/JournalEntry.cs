using AegiFinance.Domain.Enums;

namespace AegiFinance.Domain.Entities;

public class JournalEntry : AuditableEntity
{
    public string EntryNumber { get; set; } = null!;
    public DateTime Date { get; set; }
    public string Description { get; set; } = null!;
    public string? Reference { get; set; }
    public string Currency { get; set; } = "MXN";
    public JournalEntryStatus Status { get; set; } = JournalEntryStatus.Draft;
    public JournalSourceType SourceType { get; set; }
    public string? SourceId { get; set; }
    public string IdempotencyKey { get; set; } = null!;
    public Guid AccountingPeriodId { get; set; }
    public AccountingPeriod AccountingPeriod { get; set; } = null!;
    public Guid? ClientId { get; set; }
    public Client? Client { get; set; }
    public DateTime? PostedAt { get; set; }
    public Guid? PostedBy { get; set; }
    public DateTime? ReversedAt { get; set; }
    public Guid? ReversedBy { get; set; }
    public Guid? ReversesJournalEntryId { get; set; }
    public JournalEntry? ReversesJournalEntry { get; set; }
    public ICollection<JournalLine> Lines { get; set; } = new List<JournalLine>();
}
