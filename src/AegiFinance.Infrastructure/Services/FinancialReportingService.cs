using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Entities;
using AegiFinance.Domain.Enums;
using AegiFinance.Domain.Reporting;
using AegiFinance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Infrastructure.Services;

public sealed class FinancialReportingService : IFinancialReportingService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IPermissionService _permissions;

    public FinancialReportingService(ApplicationDbContext context, ICurrentUserService currentUser, IPermissionService permissions) =>
        (_context, _currentUser, _permissions) = (context, currentUser, permissions);

    public async Task<FinancialReportDto> GenerateAsync(FinancialReportQueryDto query, CancellationToken cancellationToken = default)
    {
        var organizationId = _currentUser.OrganizationId ?? throw new UnauthorizedAccessException("No hay una organización activa.");
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException("No hay un usuario activo.");
        var kind = ParseKind(query.Kind);
        if (_currentUser.ClientId.HasValue) query = query with { ClientId = _currentUser.ClientId };
        if (!await _permissions.HasPermissionAsync(userId, FinancialReportRules.RequiredPermission(kind), query.ClientId, null, cancellationToken))
            throw new UnauthorizedAccessException("No tenés permiso para consultar este reporte.");
        return await GenerateForOrganizationAsync(organizationId, query, cancellationToken);
    }

    public async Task<FinancialReportDto> GenerateForOrganizationAsync(Guid organizationId, FinancialReportQueryDto query, CancellationToken cancellationToken = default)
    {
        var kind = ParseKind(query.Kind);
        var from = query.From.Date;
        var to = query.To.Date;
        if (to < from || (to - from).TotalDays > 3660) throw new InvalidOperationException("El periodo del reporte no es válido.");
        var currency = (query.Currency ?? "MXN").Trim().ToUpperInvariant();
        if (currency.Length is < 3 or > 10) throw new InvalidOperationException("La moneda del reporte no es válida.");
        if (kind == FinancialReportKind.AccountLedger && !query.AccountId.HasValue)
            throw new InvalidOperationException("Seleccioná una cuenta contable para consultar el auxiliar.");

        var linesQuery = _context.JournalLines.IgnoreQueryFilters().AsNoTracking()
            .Where(line => !line.IsDeleted && !line.JournalEntry.IsDeleted && line.JournalEntry.AccountingPeriod.OrganizationId == organizationId &&
                line.JournalEntry.Status != JournalEntryStatus.Draft && line.JournalEntry.Currency == currency && line.JournalEntry.Date < to.AddDays(1));
        if (query.ClientId.HasValue) linesQuery = linesQuery.Where(line => line.ClientId == query.ClientId || line.JournalEntry.ClientId == query.ClientId);
        if (query.BankAccountId.HasValue) linesQuery = linesQuery.Where(line => line.BankAccountId == query.BankAccountId);
        var all = await linesQuery.Select(line => new LineData(
            line.Id, line.JournalEntryId, line.JournalEntry.EntryNumber, line.JournalEntry.Date, line.JournalEntry.Description,
            line.JournalEntry.SourceType, line.JournalEntry.ReversesJournalEntry != null ? line.JournalEntry.ReversesJournalEntry.SourceType : null,
            line.AccountId, line.Account.Code, line.Account.Name, line.Account.AccountType,
            line.Account.Purpose, line.Debit, line.Credit, line.ClientId ?? line.JournalEntry.ClientId,
            line.Client != null ? line.Client.Name : line.JournalEntry.Client != null ? line.JournalEntry.Client.Name : null,
            line.BankAccountId, line.BankAccount != null ? line.BankAccount.Name : null, line.BillingItemId,
            line.BillingItem != null ? line.BillingItem.DueDate : null)).ToListAsync(cancellationToken);

        var period = all.Where(line => line.Date >= from).ToList();
        return kind switch
        {
            FinancialReportKind.Portfolio => Portfolio(all, from, to, currency),
            FinancialReportKind.Collections => FlowReport(kind, period.Where(line => line.Purpose == GeneralLedgerAccountPurpose.Bank &&
                (line.SourceType == JournalSourceType.Payment || line.SourceType == JournalSourceType.Reversal && line.ReversesSourceType == JournalSourceType.Payment)).ToList(), from, to, currency),
            FinancialReportKind.Revenue => FlowReport(kind, period.Where(line => line.Purpose == GeneralLedgerAccountPurpose.Revenue).ToList(), from, to, currency),
            FinancialReportKind.Expenses => FlowReport(kind, period.Where(line => line.Purpose == GeneralLedgerAccountPurpose.Expense).ToList(), from, to, currency),
            FinancialReportKind.Aging => Aging(all, from, to, currency),
            FinancialReportKind.Reconciliation => await ReconciliationAsync(organizationId, query, from, to, currency, cancellationToken),
            FinancialReportKind.CashFlow => FlowReport(kind, period.Where(line => line.Purpose == GeneralLedgerAccountPurpose.Bank).ToList(), from, to, currency),
            FinancialReportKind.TrialBalance => TrialBalance(all, from, to, currency),
            FinancialReportKind.AccountLedger => AccountLedger(all.Where(line => line.AccountId == query.AccountId).ToList(), from, to, currency),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public byte[] CreateCsv(FinancialReportDto report)
    {
        var csv = new StringBuilder();
        csv.AppendLine($"Reporte,{Escape(report.Title)}");
        csv.AppendLine($"Periodo,{report.From:yyyy-MM-dd},{report.To:yyyy-MM-dd}");
        csv.AppendLine($"Unidad,{Escape(report.Unit)}");
        csv.AppendLine($"Resumen,{Escape(report.SummaryText)}");
        csv.AppendLine($"Conciliado,{report.IsReconciled},{report.ReconciliationDifference.ToString(CultureInfo.InvariantCulture)}");
        csv.AppendLine();
        csv.AppendLine(string.Join(',', report.Columns.Select(Escape)));
        foreach (var row in report.Rows)
            csv.AppendLine(string.Join(',', report.Columns.Select(column => Escape(row.Values.GetValueOrDefault(column, string.Empty)))));
        return new UTF8Encoding(true).GetBytes(csv.ToString());
    }

    public async Task<IReadOnlyList<ReportAccountDto>> GetAccountsAsync(CancellationToken cancellationToken = default) =>
        await _context.JournalLines.AsNoTracking().Where(line => line.Account.IsActive)
            .Select(line => new ReportAccountDto(line.AccountId, line.Account.Code, line.Account.Name, line.Account.Currency))
            .Distinct().OrderBy(item => item.Code).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ReportScheduleDto>> GetSchedulesAsync(CancellationToken cancellationToken = default)
    {
        var userId = RequireUser();
        var allowed = await _permissions.GetEffectivePermissionsAsync(userId, cancellationToken);
        var schedules = await _context.ReportSchedules.AsNoTracking().OrderBy(item => item.Name).ToListAsync(cancellationToken);
        return schedules.Where(item => allowed.Contains(FinancialReportRules.RequiredPermission(item.ReportKind), StringComparer.OrdinalIgnoreCase)).Select(MapSchedule).ToList();
    }

    public async Task<ReportScheduleDto> SaveScheduleAsync(Guid? id, SaveReportScheduleRequest request, CancellationToken cancellationToken = default)
    {
        var userId = RequireUser();
        var organizationId = RequireOrganization();
        if (_currentUser.ClientId.HasValue) request = request with { ClientId = _currentUser.ClientId };
        var kind = ParseKind(request.ReportKind);
        if (!await _permissions.HasPermissionAsync(userId, FinancialReportRules.RequiredPermission(kind), request.ClientId, null, cancellationToken))
            throw new UnauthorizedAccessException("No tenés permiso para programar este tipo de reporte.");
        if (request.RollingDays is < 1 or > 366) throw new InvalidOperationException("El rango móvil debe estar entre 1 y 366 días.");
        if (kind == FinancialReportKind.AccountLedger && !request.AccountId.HasValue) throw new InvalidOperationException("El auxiliar requiere una cuenta contable.");
        if (request.ClientId.HasValue && !await _context.Clients.AnyAsync(item => item.Id == request.ClientId, cancellationToken)) throw new InvalidOperationException("El cliente no existe.");
        if (request.BankAccountId.HasValue && !await _context.BankAccounts.AnyAsync(item => item.Id == request.BankAccountId, cancellationToken)) throw new InvalidOperationException("La cuenta bancaria no existe.");
        if (request.AccountId.HasValue && !await _context.GeneralLedgerAccounts.AnyAsync(item => item.Id == request.AccountId, cancellationToken)) throw new InvalidOperationException("La cuenta contable no existe.");
        if (!Enum.TryParse<ReportScheduleFrequency>(request.Frequency, true, out var frequency)) throw new InvalidOperationException("La frecuencia no es válida.");
        var next = FinancialReportRules.NextRun(DateTime.UtcNow, frequency, request.RunAtMinuteUtc, request.DayOfWeek, request.DayOfMonth);
        var entity = id.HasValue
            ? await _context.ReportSchedules.SingleOrDefaultAsync(item => item.Id == id, cancellationToken) ?? throw new KeyNotFoundException("La programación no existe.")
            : new ReportSchedule { Id = Guid.NewGuid(), OrganizationId = organizationId, CreatedByUserId = userId };
        entity.Name = request.Name.Trim();
        if (entity.Name.Length is < 3 or > 120) throw new InvalidOperationException("El nombre debe contener entre 3 y 120 caracteres.");
        entity.ReportKind = kind; entity.Frequency = frequency; entity.Currency = request.Currency.Trim().ToUpperInvariant();
        entity.RollingDays = request.RollingDays; entity.AccountId = request.AccountId; entity.ClientId = request.ClientId; entity.BankAccountId = request.BankAccountId;
        entity.RunAtMinuteUtc = request.RunAtMinuteUtc; entity.DayOfWeek = request.DayOfWeek; entity.DayOfMonth = request.DayOfMonth;
        entity.IsActive = request.IsActive; entity.NextRunAt = next;
        if (!id.HasValue) _context.ReportSchedules.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return MapSchedule(entity);
    }

    public async Task DeleteScheduleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.ReportSchedules.SingleOrDefaultAsync(item => item.Id == id, cancellationToken) ?? throw new KeyNotFoundException("La programación no existe.");
        _context.ReportSchedules.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ReportRunDto>> GetRunsAsync(CancellationToken cancellationToken = default)
    {
        var userId = RequireUser();
        var allowed = await _permissions.GetEffectivePermissionsAsync(userId, cancellationToken);
        var runs = await _context.ReportRuns.AsNoTracking().Include(item => item.ReportSchedule).OrderByDescending(item => item.StartedAt).Take(200).ToListAsync(cancellationToken);
        return runs.Where(item => allowed.Contains(FinancialReportRules.RequiredPermission(item.ReportSchedule.ReportKind), StringComparer.OrdinalIgnoreCase)).Select(MapRun).ToList();
    }

    public async Task<ReportFileDto> GetRunFileAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var run = await _context.ReportRuns.AsNoTracking().Include(item => item.ReportSchedule).SingleOrDefaultAsync(item => item.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("La ejecución no existe.");
        if (run.Status != ReportRunStatus.Completed || string.IsNullOrEmpty(run.Content)) throw new InvalidOperationException("La ejecución todavía no tiene un archivo disponible.");
        if (!await _permissions.HasPermissionAsync(RequireUser(), FinancialReportRules.RequiredPermission(run.ReportSchedule.ReportKind), run.ClientId, null, cancellationToken))
            throw new UnauthorizedAccessException("No tenés permiso para descargar este reporte.");
        return new ReportFileDto(run.FileName!, run.ContentType!, Encoding.UTF8.GetBytes(run.Content));
    }

    private static FinancialReportDto Portfolio(List<LineData> all, DateTime from, DateTime to, string currency)
    {
        var ar = all.Where(line => line.Purpose == GeneralLedgerAccountPurpose.AccountsReceivable).ToList();
        var rows = ar.GroupBy(line => new { line.ClientId, Name = line.ClientName ?? "Sin cliente" }).Select(group => new { group.Key, Amount = group.Sum(Normal) }).Where(item => item.Amount != 0).OrderByDescending(item => item.Amount).ToList();
        var total = rows.Sum(item => item.Amount);
        return Report("Portfolio", "Cartera", "Saldo pendiente por cliente derivado de las líneas de cuentas por cobrar del Major Ledger.", from, to, currency,
            $"La cartera al {to:dd/MM/yyyy} es {Money(total, currency)} en {rows.Count} clientes.", total, total,
            [Metric("portfolio", "Cartera total", total, currency, "Débitos menos créditos de cuentas por cobrar hasta la fecha final."), Metric("clients", "Clientes con saldo", rows.Count, "clientes", "Clientes cuyo auxiliar conserva un saldo distinto de cero.")],
            rows.Take(12).Select(item => new ReportSeriesPointDto(item.Key.ClientId?.ToString() ?? item.Key.Name, item.Key.Name, item.Amount)).ToList(),
            ["Cliente", "Saldo"], rows.Select(item => Row(item.Key.ClientId?.ToString() ?? item.Key.Name, ("Cliente", item.Key.Name), ("Saldo", Number(item.Amount)))).ToList());
    }

    private static FinancialReportDto Aging(List<LineData> all, DateTime from, DateTime to, string currency)
    {
        var balances = all.Where(line => line.Purpose == GeneralLedgerAccountPurpose.AccountsReceivable && line.BillingItemId.HasValue)
            .GroupBy(line => new { line.BillingItemId, line.DueDate, line.ClientName }).Select(group => new { group.Key, Amount = group.Sum(Normal) }).Where(item => item.Amount > 0).ToList();
        var buckets = new[] { ("current", "Por vencer", -99999, 0), ("1-30", "1–30 días", 1, 30), ("31-60", "31–60 días", 31, 60), ("61-90", "61–90 días", 61, 90), ("90+", "Más de 90 días", 91, 99999) };
        var results = buckets.Select(bucket => new { bucket.Item1, bucket.Item2, Amount = balances.Where(item => { var days = item.Key.DueDate.HasValue ? (to - item.Key.DueDate.Value.Date).Days : 0; return days >= bucket.Item3 && days <= bucket.Item4; }).Sum(item => item.Amount) }).ToList();
        var total = balances.Sum(item => item.Amount);
        var ledger = all.Where(line => line.Purpose == GeneralLedgerAccountPurpose.AccountsReceivable).Sum(Normal);
        return Report("Aging", "Antigüedad de saldos", "Vencimiento de cargos abiertos, conciliado contra cuentas por cobrar del Major Ledger.", from, to, currency,
            $"{Money(total, currency)} están identificados por vencimiento; diferencia contra el Mayor: {Money(ledger - total, currency)}.", total, ledger,
            [Metric("identified", "Saldo identificado", total, currency, "Saldo con cargo y fecha de vencimiento."), Metric("ledger", "Mayor de cartera", ledger, currency, "Saldo total de cuentas por cobrar en el Major Ledger.")],
            results.Select(item => new ReportSeriesPointDto(item.Item1, item.Item2, item.Amount)).ToList(), ["Rango", "Saldo"],
            results.Select(item => Row(item.Item1, ("Rango", item.Item2), ("Saldo", Number(item.Amount)))).ToList());
    }

    private static FinancialReportDto FlowReport(FinancialReportKind kind, List<LineData> lines, DateTime from, DateTime to, string currency)
    {
        decimal Signed(LineData line) => kind switch { FinancialReportKind.Revenue => line.Credit - line.Debit, FinancialReportKind.Expenses => line.Debit - line.Credit, _ => line.Debit - line.Credit };
        var byMonth = lines.GroupBy(line => line.Date.ToString("yyyy-MM")).OrderBy(group => group.Key).Select(group => new { Key = group.Key, Amount = group.Sum(Signed) }).ToList();
        var total = lines.Sum(Signed);
        var (name, definition) = kind switch
        {
            FinancialReportKind.Collections => ("Cobranza", "Entradas bancarias contabilizadas con origen de pago."),
            FinancialReportKind.Revenue => ("Ingresos", "Créditos netos de cuentas de ingresos en el Major Ledger."),
            FinancialReportKind.Expenses => ("Gastos", "Débitos netos de cuentas de gasto operativo en el Major Ledger."),
            _ => ("Flujo de efectivo", "Entradas menos salidas de cuentas bancarias según el Major Ledger.")
        };
        return Report(kind.ToString(), name, definition, from, to, currency, $"{name} neto del periodo: {Money(total, currency)}.", total, total,
            [Metric("net", $"{name} neto", total, currency, definition), Metric("movements", "Líneas contables", lines.Count, "líneas", "Cantidad de líneas del Mayor incluidas por los filtros.")],
            byMonth.Select(item => new ReportSeriesPointDto(item.Key, item.Key, item.Amount)).ToList(), ["Periodo", "Importe"],
            byMonth.Select(item => Row(item.Key, ("Periodo", item.Key), ("Importe", Number(item.Amount)))).ToList());
    }

    private static FinancialReportDto TrialBalance(List<LineData> all, DateTime from, DateTime to, string currency)
    {
        var rows = all.GroupBy(line => new { line.AccountId, line.AccountCode, line.AccountName, line.AccountType }).Select(group => new { group.Key, Debit = group.Sum(item => item.Debit), Credit = group.Sum(item => item.Credit) }).OrderBy(item => item.Key.AccountCode).ToList();
        var debit = rows.Sum(item => item.Debit); var credit = rows.Sum(item => item.Credit);
        return Report("TrialBalance", "Balanza de comprobación", "Suma de débitos y créditos contabilizados por cuenta hasta la fecha final.", from, to, currency,
            debit == credit ? $"La balanza está cuadrada en {Money(debit, currency)}." : $"La balanza tiene una diferencia de {Money(debit - credit, currency)}.", debit, credit,
            [Metric("debit", "Debe", debit, currency, "Total de débitos contabilizados."), Metric("credit", "Haber", credit, currency, "Total de créditos contabilizados.")], [], ["Cuenta", "Nombre", "Debe", "Haber", "Saldo"],
            rows.Select(item => Row(item.Key.AccountId.ToString(), ("Cuenta", item.Key.AccountCode), ("Nombre", item.Key.AccountName), ("Debe", Number(item.Debit)), ("Haber", Number(item.Credit)), ("Saldo", Number(item.Key.AccountType is GeneralLedgerAccountType.Asset or GeneralLedgerAccountType.Expense ? item.Debit - item.Credit : item.Credit - item.Debit)))).ToList());
    }

    private static FinancialReportDto AccountLedger(List<LineData> all, DateTime from, DateTime to, string currency)
    {
        var account = all.FirstOrDefault();
        decimal Signed(LineData item) => item.AccountType is GeneralLedgerAccountType.Asset or GeneralLedgerAccountType.Expense ? item.Debit - item.Credit : item.Credit - item.Debit;
        var opening = all.Where(item => item.Date < from).Sum(Signed); decimal running = opening;
        var rows = all.Where(item => item.Date >= from).OrderBy(item => item.Date).ThenBy(item => item.EntryNumber).Select(item => { running += Signed(item); return Row(item.Id.ToString(), ("Fecha", item.Date.ToString("yyyy-MM-dd")), ("Asiento", item.EntryNumber), ("Descripción", item.Description), ("Debe", Number(item.Debit)), ("Haber", Number(item.Credit)), ("Saldo", Number(running))); }).ToList();
        return Report("AccountLedger", "Auxiliar contable", "Movimientos y saldo acumulado de una cuenta del Major Ledger.", from, to, currency,
            account is null ? "La cuenta no tiene movimientos con los filtros seleccionados." : $"{account.AccountCode} · {account.AccountName}: saldo {Money(running, currency)}.", running, running,
            [Metric("opening", "Saldo inicial", opening, currency, "Saldo acumulado antes de la fecha inicial."), Metric("balance", "Saldo final", running, currency, "Saldo normal acumulado al cierre del periodo."), Metric("movements", "Movimientos", rows.Count, "líneas", "Líneas contables incluidas en el periodo.")], [], ["Fecha", "Asiento", "Descripción", "Debe", "Haber", "Saldo"], rows);
    }

    private async Task<FinancialReportDto> ReconciliationAsync(Guid organizationId, FinancialReportQueryDto query, DateTime from, DateTime to, string currency, CancellationToken cancellationToken)
    {
        var cases = _context.ReconciliationCases.IgnoreQueryFilters().AsNoTracking().Where(item => !item.IsDeleted && item.OrganizationId == organizationId && item.GeneratedAt >= from && item.GeneratedAt < to.AddDays(1) && item.BankAccount.Currency == currency);
        if (query.BankAccountId.HasValue) cases = cases.Where(item => item.BankAccountId == query.BankAccountId);
        var rows = await cases.Select(item => new { item.Id, Bank = item.BankAccount.Name, item.Status, item.BankAmount, item.LedgerAmount, item.DifferenceAmount, item.Score, item.GeneratedAt }).ToListAsync(cancellationToken);
        var confirmed = rows.Count(item => item.Status == ReconciliationStatus.Confirmed); var difference = rows.Where(item => item.Status == ReconciliationStatus.Confirmed).Sum(item => item.DifferenceAmount);
        return Report("Reconciliation", "Conciliación", "Cobertura y diferencias de casos de conciliación bancaria en el periodo.", from, to, currency,
            $"{confirmed} de {rows.Count} casos están confirmados; diferencia confirmada {Money(difference, currency)}.", difference, 0,
            [Metric("confirmed", "Casos confirmados", confirmed, "casos", "Casos aceptados por regla o confirmación humana."), Metric("difference", "Diferencia confirmada", difference, currency, "Banco menos Mayor para casos confirmados.")],
            rows.GroupBy(item => item.Status).Select(group => new ReportSeriesPointDto(group.Key.ToString(), group.Key.ToString(), group.Count())).ToList(),
            ["Fecha", "Banco", "Estado", "Importe banco", "Importe Mayor", "Diferencia", "Puntaje"], rows.OrderByDescending(item => item.GeneratedAt).Select(item => Row(item.Id.ToString(), ("Fecha", item.GeneratedAt.ToString("yyyy-MM-dd")), ("Banco", item.Bank), ("Estado", item.Status.ToString()), ("Importe banco", Number(item.BankAmount)), ("Importe Mayor", Number(item.LedgerAmount)), ("Diferencia", Number(item.DifferenceAmount)), ("Puntaje", Number(item.Score)))).ToList());
    }

    private static FinancialReportDto Report(string kind, string title, string definition, DateTime from, DateTime to, string currency, string summary, decimal derived, decimal control, IReadOnlyList<ReportMetricDto> metrics, IReadOnlyList<ReportSeriesPointDto> series, IReadOnlyList<string> columns, IReadOnlyList<ReportTableRowDto> rows)
    { var difference = derived - control; return new(kind, title, definition, from, to, currency, currency, summary, Math.Abs(difference) < 0.01m, difference, metrics, series, columns, rows); }
    private static ReportMetricDto Metric(string key, string label, decimal value, string unit, string definition) => new(key, label, value, unit, definition);
    private static ReportTableRowDto Row(string key, params (string Key, string Value)[] values) => new(key, values.ToDictionary(item => item.Key, item => item.Value));
    private static decimal Normal(LineData line) => line.Debit - line.Credit;
    private static string Number(decimal value) => value.ToString("0.00", CultureInfo.InvariantCulture);
    private static string Money(decimal value, string currency) => $"{value:N2} {currency}";
    private static string Escape(string value)
    {
        var safe = value;
        if (safe.StartsWith('=') || safe.StartsWith('+') || safe.StartsWith('@') ||
            (safe.StartsWith('-') && (safe.Length == 1 || !char.IsDigit(safe[1])))) safe = $"'{safe}";
        return $"\"{safe.Replace("\"", "\"\"")}\"";
    }
    private static FinancialReportKind ParseKind(string value) => Enum.TryParse<FinancialReportKind>(value, true, out var kind) ? kind : throw new InvalidOperationException("El tipo de reporte no es válido.");
    private Guid RequireUser() => _currentUser.UserId ?? throw new UnauthorizedAccessException("No hay un usuario activo.");
    private Guid RequireOrganization() => _currentUser.OrganizationId ?? throw new UnauthorizedAccessException("No hay una organización activa.");
    private static ReportScheduleDto MapSchedule(ReportSchedule item) => new(item.Id, item.Name, item.ReportKind.ToString(), item.Frequency.ToString(), item.Currency, item.RollingDays, item.AccountId, item.ClientId, item.BankAccountId, item.RunAtMinuteUtc, item.DayOfWeek, item.DayOfMonth, item.IsActive, item.NextRunAt, item.LastRunAt, item.CreatedByUserId);
    private static ReportRunDto MapRun(ReportRun item) => new(item.Id, item.ReportScheduleId, item.ReportSchedule.Name, item.ReportSchedule.ReportKind.ToString(), item.Status.ToString(), item.StartedAt, item.CompletedAt, item.FileName, item.ResultHash, item.RowCount, item.Error);
    private sealed record LineData(Guid Id, Guid JournalEntryId, string EntryNumber, DateTime Date, string Description, JournalSourceType SourceType, JournalSourceType? ReversesSourceType, Guid AccountId, string AccountCode, string AccountName, GeneralLedgerAccountType AccountType, GeneralLedgerAccountPurpose Purpose, decimal Debit, decimal Credit, Guid? ClientId, string? ClientName, Guid? BankAccountId, string? BankAccountName, Guid? BillingItemId, DateTime? DueDate);
}

public sealed class ReportScheduleProcessor : IReportScheduleProcessor
{
    private readonly ApplicationDbContext _context;
    private readonly IFinancialReportingService _reports;
    private readonly IPermissionService _permissions;
    public ReportScheduleProcessor(ApplicationDbContext context, IFinancialReportingService reports, IPermissionService permissions) => (_context, _reports, _permissions) = (context, reports, permissions);

    public async Task<int> ProcessDueAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var schedules = await _context.ReportSchedules.IgnoreQueryFilters().Where(item => !item.IsDeleted && item.IsActive && item.NextRunAt <= now).OrderBy(item => item.NextRunAt).Take(20).ToListAsync(cancellationToken);
        foreach (var schedule in schedules)
        {
            var run = new ReportRun { Id = Guid.NewGuid(), OrganizationId = schedule.OrganizationId, ReportScheduleId = schedule.Id, ClientId = schedule.ClientId, Status = ReportRunStatus.Pending, StartedAt = now };
            _context.ReportRuns.Add(run);
            schedule.LastRunAt = now;
            schedule.NextRunAt = FinancialReportRules.NextRun(now.AddSeconds(1), schedule.Frequency, schedule.RunAtMinuteUtc, schedule.DayOfWeek, schedule.DayOfMonth);
            try
            {
                var permission = FinancialReportRules.RequiredPermission(schedule.ReportKind);
                var activeUser = await _context.Users.IgnoreQueryFilters().AnyAsync(item => item.Id == schedule.CreatedByUserId && item.OrganizationId == schedule.OrganizationId && item.IsActive, cancellationToken);
                if (!activeUser || !await _permissions.HasPermissionAsync(schedule.CreatedByUserId, permission, schedule.ClientId, null, cancellationToken)) throw new UnauthorizedAccessException("La persona creadora ya no tiene acceso al reporte.");
                var query = new FinancialReportQueryDto(schedule.ReportKind.ToString(), now.Date.AddDays(1 - schedule.RollingDays), now.Date, schedule.Currency, schedule.ClientId, schedule.BankAccountId, schedule.AccountId);
                run.FilterJson = JsonSerializer.Serialize(query);
                var report = await _reports.GenerateForOrganizationAsync(schedule.OrganizationId, query, cancellationToken);
                var bytes = _reports.CreateCsv(report);
                run.Content = Encoding.UTF8.GetString(bytes); run.ContentType = "text/csv; charset=utf-8";
                run.FileName = $"{schedule.ReportKind.ToString().ToLowerInvariant()}-{now:yyyyMMdd-HHmm}.csv";
                run.ResultHash = Convert.ToHexString(SHA256.HashData(bytes)); run.RowCount = report.Rows.Count;
                run.Status = ReportRunStatus.Completed; run.CompletedAt = DateTime.UtcNow;
            }
            catch (Exception exception)
            {
                run.Status = ReportRunStatus.Failed; run.CompletedAt = DateTime.UtcNow; run.Error = exception.Message.Length > 2000 ? exception.Message[..2000] : exception.Message;
            }
            await _context.SaveChangesAsync(cancellationToken);
        }
        return schedules.Count;
    }
}
