using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Dashboard;

public sealed class GetDashboardAttentionQuery : IRequest<DashboardAttentionDto>
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public Guid? BankAccountId { get; set; }
    public Guid? ClientId { get; set; }
    public string Currency { get; set; } = "MXN";
}

public sealed class GetDashboardAttentionQueryHandler : IRequestHandler<GetDashboardAttentionQuery, DashboardAttentionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IPermissionService _permissions;

    public GetDashboardAttentionQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser, IPermissionService permissions)
    {
        _context = context;
        _currentUser = currentUser;
        _permissions = permissions;
    }

    public async Task<DashboardAttentionDto> Handle(GetDashboardAttentionQuery request, CancellationToken cancellationToken)
    {
        var normalized = GetDashboardSummaryQueryHandler.Normalize(new GetDashboardSummaryQuery
        {
            From = request.From, To = request.To, ClientId = request.ClientId, Currency = request.Currency
        }, _currentUser);
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var permissionCodes = await _permissions.GetEffectivePermissionsAsync(userId, normalized.ClientId, null, cancellationToken);
        var result = new DashboardAttentionDto { GeneratedAt = DateTime.UtcNow };

        if (permissionCodes.Contains("ViewPayments", StringComparer.OrdinalIgnoreCase))
        {
            var income = _context.LedgerEntries.AsNoTracking()
                .ApplyOperationalScope(normalized.ClientId, request.BankAccountId, normalized.From, normalized.To, normalized.Currency)
                .Where(item => item.EntryType == LedgerEntryType.Income);
            var unapplied = await income.Select(item => new
            {
                item.Amount,
                Allocated = _context.SubscriptionAllocations.Where(allocation => allocation.LedgerEntryId == item.Id && !allocation.IsReversed)
                    .Sum(allocation => (decimal?)allocation.Amount) ?? 0
            }).Where(item => item.Allocated < item.Amount).ToListAsync(cancellationToken);
            if (unapplied.Count > 0)
            {
                result.Items.Add(new DashboardAttentionItemDto
                {
                    Kind = "UnappliedPayments", Severity = "Attention", Title = "Pagos sin aplicar",
                    Detail = "Ingresos disponibles que todavía no están relacionados completamente con cargos.",
                    Count = unapplied.Count, Amount = unapplied.Sum(item => item.Amount - item.Allocated), Currency = normalized.Currency,
                    Href = "/ledger?entryType=Income&hasUnappliedBalance=true", Permission = "ViewPayments"
                });
            }

            var overdue = _context.BillingItems.AsNoTracking()
                .ApplyOperationalScope(normalized.ClientId, normalized.From, normalized.To, normalized.Currency)
                .Outstanding().Where(item => item.DueDate < DateTime.UtcNow.Date);
            var overdueData = await overdue.Select(item => item.Amount - item.PaidAmount
                + (item.Adjustments.Where(x => !x.ReversedAt.HasValue && x.Type == BillingAdjustmentType.LateFee).Sum(x => (decimal?)x.Amount) ?? 0m)
                - (item.Adjustments.Where(x => !x.ReversedAt.HasValue && x.Type == BillingAdjustmentType.CreditNote).Sum(x => (decimal?)x.Amount) ?? 0m)).ToListAsync(cancellationToken);
            if (overdueData.Count > 0)
            {
                result.Items.Add(new DashboardAttentionItemDto
                {
                    Kind = "OverdueCharges", Severity = "Risk", Title = "Cargos vencidos",
                    Detail = "Saldos vencidos que requieren seguimiento o aplicación de pago.", Count = overdueData.Count,
                    Amount = overdueData.Sum(), Currency = normalized.Currency,
                    Href = "/billing?outstandingOnly=true", Permission = "ViewPayments"
                });
            }
        }

        if (permissionCodes.Contains("ManageReconciliation", StringComparer.OrdinalIgnoreCase))
        {
            var lines = _context.BankStatementLines.AsNoTracking().Where(item => !item.IsReconciled &&
                item.TransactionDate >= normalized.From && item.TransactionDate < normalized.To.AddDays(1) &&
                item.BankStatement.BankAccount.Currency == normalized.Currency);
            if (request.BankAccountId.HasValue) lines = lines.Where(item => item.BankStatement.BankAccountId == request.BankAccountId.Value);
            var differenceCount = await lines.CountAsync(cancellationToken);
            if (differenceCount > 0)
            {
                result.Items.Add(new DashboardAttentionItemDto
                {
                    Kind = "ReconciliationDifferences", Severity = "Attention", Title = "Diferencias por conciliar",
                    Detail = "Líneas bancarias que todavía no tienen conciliación confirmada.", Count = differenceCount,
                    Currency = normalized.Currency, Href = "/ledger?view=reconciliation", Permission = "ManageReconciliation"
                });
            }

            var failedImports = _context.BankImportAttempts.AsNoTracking().Where(item => item.Status == AegiFinance.Domain.Enums.BankImportStatus.Failed &&
                item.AttemptedAt >= normalized.From && item.AttemptedAt < normalized.To.AddDays(1) &&
                item.BankAccount.Currency == normalized.Currency);
            if (request.BankAccountId.HasValue) failedImports = failedImports.Where(item => item.BankAccountId == request.BankAccountId.Value);
            var failedCount = await failedImports.CountAsync(cancellationToken);
            if (failedCount > 0)
            {
                result.Items.Add(new DashboardAttentionItemDto
                {
                    Kind = "ImportFailures", Severity = "Risk", Title = "Importaciones fallidas",
                    Detail = "Archivos bancarios rechazados que conservan evidencia y motivo del fallo.", Count = failedCount,
                    Currency = normalized.Currency, Href = "/ledger?view=imports&status=failed", Permission = "ManageReconciliation"
                });
            }
        }

        return result;
    }
}
