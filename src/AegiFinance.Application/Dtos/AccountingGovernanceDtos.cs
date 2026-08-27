namespace AegiFinance.Application.Dtos;

public sealed record AccountingCloseCheckDto(string Key, string Label, string State, int Count, string Detail, bool BlocksClose);
public sealed record AccountingPeriodChecklistDto(Guid PeriodId, string PeriodName, bool CanClose, string VerificationCode, IReadOnlyList<AccountingCloseCheckDto> Items);
public sealed record AccountingIntegrityAlertDto(string Key, string Severity, string Title, string Detail, int Count);
public sealed record AccountingEvidenceSummaryDto(int AuditEvents, int AccessEvents, int CommittedImports, int FailedImports, int RolledBackImports, int AutomaticReconciliations, int JournalReversals, int PaymentReversals, DateTime? LastEventAt);
public sealed record AccountingPeriodReopenRequestDto(Guid Id, Guid PeriodId, string PeriodName, string Reason, string Status, DateTime RequestedAt, Guid RequestedBy, string? RequestedByName, DateTime? ReviewedAt, Guid? ReviewedBy, string? ReviewedByName, string? ReviewComment);
public sealed record CloseAccountingPeriodRequest(string VerificationCode);
public sealed record RequestAccountingPeriodReopenRequest(string Reason);
public sealed record ReviewAccountingPeriodReopenRequest(bool Approve, string Comment);
