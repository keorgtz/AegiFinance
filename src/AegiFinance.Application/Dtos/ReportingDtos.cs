namespace AegiFinance.Application.Dtos;

public sealed record FinancialReportQueryDto(string Kind, DateTime From, DateTime To, string Currency, Guid? ClientId = null, Guid? BankAccountId = null, Guid? AccountId = null);
public sealed record ReportMetricDto(string Key, string Label, decimal Value, string Unit, string Definition);
public sealed record ReportSeriesPointDto(string Key, string Label, decimal Value, decimal? SecondaryValue = null);
public sealed record ReportTableRowDto(string Key, IReadOnlyDictionary<string, string> Values);
public sealed record FinancialReportDto(string Kind, string Title, string Definition, DateTime From, DateTime To, string Currency, string Unit,
    string SummaryText, bool IsReconciled, decimal ReconciliationDifference, IReadOnlyList<ReportMetricDto> Metrics,
    IReadOnlyList<ReportSeriesPointDto> Series, IReadOnlyList<string> Columns, IReadOnlyList<ReportTableRowDto> Rows);

public sealed record ReportScheduleDto(Guid Id, string Name, string ReportKind, string Frequency, string Currency, int RollingDays,
    Guid? AccountId, Guid? ClientId, Guid? BankAccountId, int RunAtMinuteUtc, int? DayOfWeek, int? DayOfMonth,
    bool IsActive, DateTime NextRunAt, DateTime? LastRunAt, Guid CreatedByUserId);
public sealed record ReportRunDto(Guid Id, Guid ReportScheduleId, string ScheduleName, string ReportKind, string Status,
    DateTime StartedAt, DateTime? CompletedAt, string? FileName, string? ResultHash, int RowCount, string? Error);
public sealed record SaveReportScheduleRequest(string Name, string ReportKind, string Frequency, string Currency, int RollingDays,
    Guid? AccountId, Guid? ClientId, Guid? BankAccountId, int RunAtMinuteUtc, int? DayOfWeek, int? DayOfMonth, bool IsActive);
public sealed record ReportFileDto(string FileName, string ContentType, byte[] Content);
public sealed record ReportAccountDto(Guid Id, string Code, string Name, string Currency);
