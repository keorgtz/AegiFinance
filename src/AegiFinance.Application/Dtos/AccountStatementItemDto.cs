namespace AegiFinance.Application.Dtos;

public class AccountStatementItemDto
{
    public Guid Id { get; set; }
    public Guid JournalEntryId { get; set; }
    public string JournalEntryNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Type { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Reference { get; set; }
    public Guid ReferenceId { get; set; }
    public Guid? SubscriptionId { get; set; }
    public string? SubscriptionCode { get; set; }
    public string? ServiceName { get; set; }
    public DateTime? DueDate { get; set; }
    public bool IsOverdue { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal Balance { get; set; }
    public string Currency { get; set; } = "MXN";
}
