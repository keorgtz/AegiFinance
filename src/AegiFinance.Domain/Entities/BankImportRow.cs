using AegiFinance.Domain.Enums;

namespace AegiFinance.Domain.Entities;

public sealed class BankImportRow : AuditableEntity
{
    public Guid ImportAttemptId { get; set; }
    public BankImportAttempt ImportAttempt { get; set; } = null!;
    public int RowNumber { get; set; }
    public string RawDataJson { get; set; } = "{}";
    public DateTime? TransactionDate { get; set; }
    public string? Description { get; set; }
    public string? Reference { get; set; }
    public decimal? Amount { get; set; }
    public string? Currency { get; set; }
    public decimal? Balance { get; set; }
    public string? DeduplicationHash { get; set; }
    public BankImportRowStatus Status { get; set; }
    public string IssuesJson { get; set; } = "[]";
    public Guid? BankStatementLineId { get; set; }
    public BankStatementLine? BankStatementLine { get; set; }
}
