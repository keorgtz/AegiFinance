namespace AegiFinance.Domain.Entities;

public sealed class BankImportAttempt : AuditableEntity
{
    public Guid BankAccountId { get; set; }
    public BankAccount BankAccount { get; set; } = null!;
    public string FileName { get; set; } = string.Empty;
    public string Status { get; set; } = "Processing";
    public string? Error { get; set; }
    public int RecordsImported { get; set; }
    public DateTime AttemptedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
