using AegiFinance.Domain.Enums;

namespace AegiFinance.Application.Features.BankImports;

public sealed class BankImportIssueDto
{
    public string Field { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string Correction { get; init; } = string.Empty;
}

public sealed class BankImportRowDto
{
    public Guid Id { get; init; }
    public int RowNumber { get; init; }
    public DateTime? TransactionDate { get; init; }
    public string? Description { get; init; }
    public string? Reference { get; init; }
    public decimal? Amount { get; init; }
    public string? Currency { get; init; }
    public decimal? Balance { get; init; }
    public BankImportRowStatus Status { get; init; }
    public IReadOnlyList<BankImportIssueDto> Issues { get; init; } = [];
}

public sealed class BankImportBatchDto
{
    public Guid Id { get; init; }
    public Guid BankAccountId { get; init; }
    public string BankAccountName { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public string FileHash { get; init; } = string.Empty;
    public string AdapterCode { get; init; } = string.Empty;
    public BankImportStatus Status { get; init; }
    public int TotalRecords { get; init; }
    public int ValidRecords { get; init; }
    public int DuplicateRecords { get; init; }
    public int IncompleteRecords { get; init; }
    public int RejectedRecords { get; init; }
    public int RecordsImported { get; init; }
    public Guid? BankStatementId { get; init; }
    public DateTime AttemptedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public IReadOnlyList<BankImportRowDto> Rows { get; init; } = [];
}

public sealed class BankImportProfileDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string AdapterCode { get; init; } = string.Empty;
    public string DateColumn { get; init; } = string.Empty;
    public string DescriptionColumn { get; init; } = string.Empty;
    public string? ReferenceColumn { get; init; }
    public string? AmountColumn { get; init; }
    public string? DebitColumn { get; init; }
    public string? CreditColumn { get; init; }
    public string? CurrencyColumn { get; init; }
    public string? BalanceColumn { get; init; }
    public string? DateFormat { get; init; }
    public string Delimiter { get; init; } = ",";
    public int HeaderRow { get; init; }
}

public sealed class BankImportColumnMap
{
    public string? Date { get; set; }
    public string? Description { get; set; }
    public string? Reference { get; set; }
    public string? Amount { get; set; }
    public string? Debit { get; set; }
    public string? Credit { get; set; }
    public string? Currency { get; set; }
    public string? Balance { get; set; }
}

public sealed record BankImportAdapterDto(string Code, string Name, string Description);
