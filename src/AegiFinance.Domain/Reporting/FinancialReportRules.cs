using AegiFinance.Domain.Enums;

namespace AegiFinance.Domain.Reporting;

public static class FinancialReportRules
{
    public static string RequiredPermission(FinancialReportKind kind) => kind switch
    {
        FinancialReportKind.Portfolio or FinancialReportKind.Aging => "ViewReceivablesReports",
        FinancialReportKind.Collections => "ViewCollectionsReports",
        FinancialReportKind.Revenue or FinancialReportKind.Expenses or FinancialReportKind.CashFlow => "ViewFinancialReports",
        FinancialReportKind.Reconciliation => "ViewReconciliationReports",
        FinancialReportKind.TrialBalance or FinancialReportKind.AccountLedger => "ViewAccountingReports",
        _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };

    public static DateTime NextRun(DateTime afterUtc, ReportScheduleFrequency frequency, int runAtMinuteUtc, int? dayOfWeek, int? dayOfMonth)
    {
        if (runAtMinuteUtc is < 0 or > 1439) throw new InvalidOperationException("La hora UTC programada no es válida.");
        var time = TimeSpan.FromMinutes(runAtMinuteUtc);
        if (frequency == ReportScheduleFrequency.Daily)
        {
            var candidate = afterUtc.Date.Add(time);
            return candidate > afterUtc ? candidate : candidate.AddDays(1);
        }
        if (frequency == ReportScheduleFrequency.Weekly)
        {
            var target = dayOfWeek ?? throw new InvalidOperationException("La programación semanal requiere un día de la semana.");
            if (target is < 0 or > 6) throw new InvalidOperationException("El día de la semana no es válido.");
            var days = (target - (int)afterUtc.DayOfWeek + 7) % 7;
            var candidate = afterUtc.Date.AddDays(days).Add(time);
            return candidate > afterUtc ? candidate : candidate.AddDays(7);
        }
        var day = Math.Clamp(dayOfMonth ?? throw new InvalidOperationException("La programación mensual requiere un día del mes."), 1, 28);
        var monthly = new DateTime(afterUtc.Year, afterUtc.Month, day, 0, 0, 0, DateTimeKind.Utc).Add(time);
        return monthly > afterUtc ? monthly : new DateTime(afterUtc.AddMonths(1).Year, afterUtc.AddMonths(1).Month, day, 0, 0, 0, DateTimeKind.Utc).Add(time);
    }
}
