using System.Data;
using System.Text.Json;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Accounting;
using AegiFinance.Domain.Entities;
using AegiFinance.Domain.Enums;
using AegiFinance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Infrastructure.Services;

public sealed class AccountingGovernanceService : IAccountingGovernanceService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public AccountingGovernanceService(ApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<AccountingPeriodDto>> GetPeriodsAsync(CancellationToken cancellationToken = default) =>
        await _context.AccountingPeriods.AsNoTracking().OrderByDescending(item => item.StartDate).Select(item =>
            new AccountingPeriodDto(item.Id, item.Name, item.StartDate, item.EndDate, item.Status.ToString(), item.ClosedAt,
                item.ClosedBy, item.CloseVerificationCode, item.ReopenedAt, item.ReopenedBy, item.ReopenReason, item.GovernanceVersion)).ToListAsync(cancellationToken);

    public async Task<AccountingPeriodChecklistDto> GetChecklistAsync(Guid periodId, CancellationToken cancellationToken = default)
    {
        var period = await LoadPeriodAsync(periodId, false, cancellationToken);
        return await BuildChecklistAsync(period, cancellationToken);
    }

    public async Task<AccountingPeriodDto> CloseAsync(Guid periodId, string verificationCode, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var period = await LoadPeriodAsync(periodId, true, cancellationToken);
        if (period.Status == AccountingPeriodStatus.Closed) return MapPeriod(period);
        var checklist = await BuildChecklistAsync(period, cancellationToken);
        if (!checklist.CanClose) throw new InvalidOperationException("El periodo conserva bloqueos en su checklist de cierre.");
        if (!string.Equals(checklist.VerificationCode, verificationCode?.Trim(), StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("El checklist cambió; revisalo nuevamente antes de cerrar.");
        period.Status = AccountingPeriodStatus.Closed;
        period.ClosedAt = DateTime.UtcNow;
        period.ClosedBy = RequireUserId();
        period.CloseChecklistJson = JsonSerializer.Serialize(checklist.Items);
        period.CloseVerificationCode = checklist.VerificationCode;
        period.GovernanceVersion++;
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return MapPeriod(period);
    }

    public async Task<AccountingPeriodReopenRequestDto> RequestReopenAsync(Guid periodId, string reason, CancellationToken cancellationToken = default)
    {
        var period = await LoadPeriodAsync(periodId, false, cancellationToken);
        if (period.Status != AccountingPeriodStatus.Closed) throw new InvalidOperationException("Sólo puede solicitarse la reapertura de un periodo cerrado.");
        var normalized = reason.Trim();
        if (normalized.Length is < 10 or > 2000) throw new InvalidOperationException("El motivo debe contener entre 10 y 2000 caracteres.");
        var pending = await _context.AccountingPeriodReopenRequests.Include(item => item.AccountingPeriod)
            .FirstOrDefaultAsync(item => item.AccountingPeriodId == periodId && item.Status == AccountingPeriodReopenStatus.Pending, cancellationToken);
        if (pending is not null)
        {
            if (pending.RequestedBy != RequireUserId()) throw new InvalidOperationException("El periodo ya tiene una solicitud de reapertura pendiente.");
            return await MapRequestAsync(pending, cancellationToken);
        }
        var request = new AccountingPeriodReopenRequest
        {
            Id = Guid.NewGuid(), OrganizationId = RequireOrganizationId(), AccountingPeriodId = periodId,
            Reason = normalized, Status = AccountingPeriodReopenStatus.Pending,
            RequestedAt = DateTime.UtcNow, RequestedBy = RequireUserId(), AccountingPeriod = period
        };
        _context.AccountingPeriodReopenRequests.Add(request);
        await _context.SaveChangesAsync(cancellationToken);
        return await MapRequestAsync(request, cancellationToken);
    }

    public async Task<AccountingPeriodReopenRequestDto> ReviewReopenAsync(Guid requestId, bool approve, string comment, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var request = await _context.AccountingPeriodReopenRequests.Include(item => item.AccountingPeriod)
            .SingleOrDefaultAsync(item => item.Id == requestId, cancellationToken) ?? throw new KeyNotFoundException("La solicitud de reapertura no existe.");
        if (request.Status != AccountingPeriodReopenStatus.Pending) throw new InvalidOperationException("La solicitud ya fue revisada.");
        var reviewerId = RequireUserId();
        AccountingGovernanceRules.EnsureIndependentApproval(request.RequestedBy, reviewerId);
        var normalized = comment.Trim();
        if (normalized.Length is < 5 or > 2000) throw new InvalidOperationException("El comentario de revisión debe contener entre 5 y 2000 caracteres.");
        request.Status = approve ? AccountingPeriodReopenStatus.Approved : AccountingPeriodReopenStatus.Rejected;
        request.ReviewedAt = DateTime.UtcNow; request.ReviewedBy = reviewerId; request.ReviewComment = normalized;
        if (approve)
        {
            if (request.AccountingPeriod.Status != AccountingPeriodStatus.Closed) throw new InvalidOperationException("El periodo ya no está cerrado.");
            request.AccountingPeriod.Status = AccountingPeriodStatus.Open;
            request.AccountingPeriod.ReopenedAt = request.ReviewedAt;
            request.AccountingPeriod.ReopenedBy = reviewerId;
            request.AccountingPeriod.ReopenReason = request.Reason;
            request.AccountingPeriod.GovernanceVersion++;
        }
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return await MapRequestAsync(request, cancellationToken);
    }

    public async Task<IReadOnlyList<AccountingPeriodReopenRequestDto>> GetReopenRequestsAsync(CancellationToken cancellationToken = default)
    {
        var requests = await _context.AccountingPeriodReopenRequests.AsNoTracking().Include(item => item.AccountingPeriod)
            .OrderByDescending(item => item.RequestedAt).Take(200).ToListAsync(cancellationToken);
        var userIds = requests.SelectMany(item => new Guid?[] { item.RequestedBy, item.ReviewedBy }).Where(item => item.HasValue).Select(item => item!.Value).Distinct().ToList();
        var names = await _context.Users.AsNoTracking().Where(item => userIds.Contains(item.Id)).ToDictionaryAsync(item => item.Id, item => item.Name, cancellationToken);
        return requests.Select(item => MapRequest(item, names)).ToList();
    }

    public async Task<IReadOnlyList<AccountingIntegrityAlertDto>> GetIntegrityAlertsAsync(CancellationToken cancellationToken = default)
    {
        var snapshot = await SnapshotAsync(null, cancellationToken);
        var alerts = new List<AccountingIntegrityAlertDto>();
        AddAlert(alerts, "unbalanced", "Critical", "Asientos descuadrados", "El debe y el haber no coinciden.", snapshot.UnbalancedEntries);
        AddAlert(alerts, "orphans", "Critical", "Asientos huérfanos", "Existen asientos contabilizados sin líneas válidas.", snapshot.OrphanEntries);
        AddAlert(alerts, "drafts", "Warning", "Borradores pendientes", "Hay asientos que todavía no fueron contabilizados.", snapshot.DraftEntries);
        AddAlert(alerts, "pending-imports", "Warning", "Importaciones pendientes", "Hay vistas previas bancarias sin confirmar ni revertir.", snapshot.PendingImports);
        AddAlert(alerts, "failed-imports", "Warning", "Importaciones fallidas", "Revisá la evidencia y el error de cada intento.", snapshot.FailedImports);
        AddAlert(alerts, "unreconciled", "Warning", "Movimientos sin conciliar", "Quedan partidas bancarias pendientes de conciliación.", snapshot.UnreconciledBankLines);
        return alerts;
    }

    public async Task<AccountingEvidenceSummaryDto> GetEvidenceAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        if (from.HasValue && to.HasValue && from.Value.Date > to.Value.Date) throw new InvalidOperationException("La fecha inicial no puede ser posterior a la fecha final.");
        var start = from?.Date; var end = to?.Date.AddDays(1);
        var audits = _context.AuditLogs.AsNoTracking().AsQueryable();
        var imports = _context.BankImportAttempts.AsNoTracking().AsQueryable();
        var reconciliations = _context.ReconciliationCases.AsNoTracking().AsQueryable();
        var journals = _context.JournalEntries.AsNoTracking().AsQueryable();
        var payments = _context.PaymentApplications.AsNoTracking().AsQueryable();
        if (start.HasValue) { audits = audits.Where(item => item.Timestamp >= start); imports = imports.Where(item => item.AttemptedAt >= start); reconciliations = reconciliations.Where(item => item.GeneratedAt >= start); journals = journals.Where(item => item.Date >= start); payments = payments.Where(item => item.CreatedAt >= start); }
        if (end.HasValue) { audits = audits.Where(item => item.Timestamp < end); imports = imports.Where(item => item.AttemptedAt < end); reconciliations = reconciliations.Where(item => item.GeneratedAt < end); journals = journals.Where(item => item.Date < end); payments = payments.Where(item => item.CreatedAt < end); }
        return new AccountingEvidenceSummaryDto(
            await audits.CountAsync(cancellationToken),
            await audits.CountAsync(item => item.EntityType == nameof(UserSession), cancellationToken),
            await imports.CountAsync(item => item.Status == BankImportStatus.Committed, cancellationToken),
            await imports.CountAsync(item => item.Status == BankImportStatus.Failed, cancellationToken),
            await imports.CountAsync(item => item.Status == BankImportStatus.RolledBack, cancellationToken),
            await reconciliations.CountAsync(item => item.IsAutomatic, cancellationToken),
            await journals.CountAsync(item => item.ReversedAt.HasValue || item.SourceType == JournalSourceType.Reversal, cancellationToken),
            await payments.CountAsync(item => item.ReversedAt.HasValue, cancellationToken),
            await audits.MaxAsync(item => (DateTime?)item.Timestamp, cancellationToken));
    }

    private async Task<AccountingPeriodChecklistDto> BuildChecklistAsync(AccountingPeriod period, CancellationToken cancellationToken)
    {
        var snapshot = await SnapshotAsync(period, cancellationToken);
        var checks = AccountingGovernanceRules.BuildCloseChecklist(period.EndDate, DateTime.UtcNow, snapshot);
        var code = AccountingGovernanceRules.VerificationCode(period.Id, snapshot, checks);
        return new AccountingPeriodChecklistDto(period.Id, period.Name, AccountingGovernanceRules.CanClose(checks), code,
            checks.Select(item => new AccountingCloseCheckDto(item.Key, item.Label, item.State, item.Count, item.Detail, item.BlocksClose)).ToList());
    }

    private async Task<AccountingIntegritySnapshot> SnapshotAsync(AccountingPeriod? period, CancellationToken cancellationToken)
    {
        var entries = _context.JournalEntries.AsNoTracking().AsQueryable();
        var imports = _context.BankImportAttempts.AsNoTracking().AsQueryable();
        var bankLines = _context.BankStatementLines.AsNoTracking().AsQueryable();
        if (period is not null)
        {
            entries = entries.Where(item => item.AccountingPeriodId == period.Id);
            imports = imports.Where(item => item.AttemptedAt >= period.StartDate && item.AttemptedAt <= period.EndDate);
            bankLines = bankLines.Where(item => item.TransactionDate >= period.StartDate && item.TransactionDate <= period.EndDate);
        }
        var totals = await entries.Where(item => item.Status != JournalEntryStatus.Draft)
            .Select(item => new { Lines = item.Lines.Count, Debit = item.Lines.Sum(line => (decimal?)line.Debit) ?? 0m, Credit = item.Lines.Sum(line => (decimal?)line.Credit) ?? 0m })
            .ToListAsync(cancellationToken);
        return new AccountingIntegritySnapshot(
            await entries.CountAsync(item => item.Status == JournalEntryStatus.Draft, cancellationToken),
            totals.Count(item => item.Lines > 0 && item.Debit != item.Credit),
            totals.Count(item => item.Lines == 0),
            await imports.CountAsync(item => item.Status == BankImportStatus.Preview, cancellationToken),
            await imports.CountAsync(item => item.Status == BankImportStatus.Failed, cancellationToken),
            await bankLines.CountAsync(item => !item.IsReconciled, cancellationToken));
    }

    private async Task<AccountingPeriod> LoadPeriodAsync(Guid id, bool tracked, CancellationToken cancellationToken)
    {
        IQueryable<AccountingPeriod> query = _context.AccountingPeriods;
        if (!tracked) query = query.AsNoTracking();
        return await query.SingleOrDefaultAsync(item => item.Id == id, cancellationToken) ?? throw new KeyNotFoundException("El periodo contable no existe.");
    }

    private async Task<AccountingPeriodReopenRequestDto> MapRequestAsync(AccountingPeriodReopenRequest request, CancellationToken cancellationToken)
    {
        var ids = new Guid?[] { request.RequestedBy, request.ReviewedBy }.Where(item => item.HasValue).Select(item => item!.Value).Distinct().ToList();
        var names = await _context.Users.AsNoTracking().Where(item => ids.Contains(item.Id)).ToDictionaryAsync(item => item.Id, item => item.Name, cancellationToken);
        return MapRequest(request, names);
    }

    private static AccountingPeriodReopenRequestDto MapRequest(AccountingPeriodReopenRequest item, IReadOnlyDictionary<Guid, string> names) =>
        new(item.Id, item.AccountingPeriodId, item.AccountingPeriod.Name, item.Reason, item.Status.ToString(), item.RequestedAt, item.RequestedBy,
            names.GetValueOrDefault(item.RequestedBy), item.ReviewedAt, item.ReviewedBy, item.ReviewedBy.HasValue ? names.GetValueOrDefault(item.ReviewedBy.Value) : null, item.ReviewComment);

    private static AccountingPeriodDto MapPeriod(AccountingPeriod period) => new(period.Id, period.Name, period.StartDate, period.EndDate, period.Status.ToString(), period.ClosedAt,
        period.ClosedBy, period.CloseVerificationCode, period.ReopenedAt, period.ReopenedBy, period.ReopenReason, period.GovernanceVersion);

    private static void AddAlert(ICollection<AccountingIntegrityAlertDto> alerts, string key, string severity, string title, string detail, int count)
    { if (count > 0) alerts.Add(new AccountingIntegrityAlertDto(key, severity, title, detail, count)); }
    private Guid RequireOrganizationId() => _currentUser.OrganizationId ?? throw new UnauthorizedAccessException("No hay una organización activa.");
    private Guid RequireUserId() => _currentUser.UserId ?? throw new UnauthorizedAccessException("No hay un usuario activo.");
}
