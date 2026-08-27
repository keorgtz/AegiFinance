using AegiFinance.Domain.Enums;

namespace AegiFinance.Domain.Entities;

public sealed class BankImportAttempt : AuditableEntity
{
    public Guid BankAccountId { get; set; }
    public BankAccount BankAccount { get; set; } = null!;
    public string FileName { get; set; } = string.Empty;
    public BankImportStatus Status { get; set; } = BankImportStatus.Preview;
    public string? Error { get; set; }
    public string FileHash { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FileType { get; set; } = string.Empty;
    public string AdapterCode { get; set; } = "generic";
    public Guid? ProfileId { get; set; }
    public BankImportProfile? Profile { get; set; }
    public Guid? BankStatementId { get; set; }
    public BankStatement? BankStatement { get; set; }
    public int TotalRecords { get; set; }
    public int ValidRecords { get; set; }
    public int DuplicateRecords { get; set; }
    public int IncompleteRecords { get; set; }
    public int RejectedRecords { get; set; }
    public int RecordsImported { get; set; }
    public DateTime AttemptedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? RolledBackAt { get; set; }
    public Guid? RolledBackBy { get; set; }
    public ICollection<BankImportRow> Rows { get; set; } = new List<BankImportRow>();
}
