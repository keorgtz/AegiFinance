namespace AegiFinance.Application.Dtos;

public sealed class DashboardMetricDto
{
    public int Count { get; set; }
    public decimal? Amount { get; set; }
    public string Currency { get; set; } = "MXN";
}

public sealed class DashboardSummaryDto
{
    public DashboardMetricDto? ActiveClients { get; set; }
    public DashboardMetricDto? OutstandingCharges { get; set; }
    public DashboardMetricDto? OverdueCharges { get; set; }
    public DashboardMetricDto? LedgerMovements { get; set; }
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public string Currency { get; set; } = "MXN";
}

public sealed class DashboardAttentionItemDto
{
    public string Kind { get; set; } = string.Empty;
    public string Severity { get; set; } = "Attention";
    public string Title { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal? Amount { get; set; }
    public string Currency { get; set; } = "MXN";
    public string Href { get; set; } = string.Empty;
    public string Permission { get; set; } = string.Empty;
}

public sealed class DashboardAttentionDto
{
    public List<DashboardAttentionItemDto> Items { get; set; } = [];
    public DateTime GeneratedAt { get; set; }
}

public sealed class DashboardTrendPointDto
{
    public DateTime PeriodStart { get; set; }
    public string Label { get; set; } = string.Empty;
    public decimal Charges { get; set; }
    public decimal Payments { get; set; }
}

public sealed class DashboardActivityDto
{
    public List<DashboardTrendPointDto> Trend { get; set; } = [];
    public List<LedgerEntryListDto> RecentMovements { get; set; } = [];
    public string Summary { get; set; } = string.Empty;
    public string Currency { get; set; } = "MXN";
    public DateTime From { get; set; }
    public DateTime To { get; set; }
}

public sealed class BankImportAttemptDto
{
    public Guid Id { get; set; }
    public string BankAccountName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Error { get; set; }
    public int RecordsImported { get; set; }
    public DateTime AttemptedAt { get; set; }
}

public sealed class UnreconciledBankLineDto
{
    public Guid Id { get; set; }
    public string BankAccountName { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "MXN";
    public string? Reference { get; set; }
}
