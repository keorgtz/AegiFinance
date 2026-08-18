using System.Globalization;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Dashboard;

public sealed class GetDashboardActivityQuery : IRequest<DashboardActivityDto>
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public Guid? BankAccountId { get; set; }
    public Guid? ClientId { get; set; }
    public string Currency { get; set; } = "MXN";
}

public sealed class GetDashboardActivityQueryHandler : IRequestHandler<GetDashboardActivityQuery, DashboardActivityDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IPermissionService _permissions;

    public GetDashboardActivityQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser, IPermissionService permissions)
    {
        _context = context;
        _currentUser = currentUser;
        _permissions = permissions;
    }

    public async Task<DashboardActivityDto> Handle(GetDashboardActivityQuery request, CancellationToken cancellationToken)
    {
        var normalized = GetDashboardSummaryQueryHandler.Normalize(new GetDashboardSummaryQuery
        {
            From = request.From, To = request.To, ClientId = request.ClientId, Currency = request.Currency
        }, _currentUser);
        var result = new DashboardActivityDto { From = normalized.From, To = normalized.To, Currency = normalized.Currency };
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var permissionCodes = await _permissions.GetEffectivePermissionsAsync(userId, normalized.ClientId, null, cancellationToken);
        if (!permissionCodes.Contains("ViewPayments", StringComparer.OrdinalIgnoreCase))
        {
            result.Summary = "Tu rol no tiene acceso a la actividad financiera de este periodo.";
            return result;
        }

        var ledger = _context.LedgerEntries.AsNoTracking().Include(item => item.BankAccount).Include(item => item.Client)
            .ApplyOperationalScope(normalized.ClientId, request.BankAccountId, normalized.From, normalized.To, normalized.Currency);
        result.RecentMovements = await ledger.OrderByDescending(item => item.Date).ThenByDescending(item => item.CreatedAt).Take(8)
            .Select(item => new LedgerEntryListDto
            {
                Id = item.Id, BankAccountId = item.BankAccountId, BankAccountName = item.BankAccount.Name,
                EntryType = item.EntryType.ToString(), Amount = item.Amount, Currency = item.Currency, Date = item.Date,
                Description = item.Description, Reference = item.Reference, ClientId = item.ClientId,
                ClientName = item.Client != null ? item.Client.Name : null, IsReconciled = item.IsReconciled
            }).ToListAsync(cancellationToken);

        var chargeDays = await _context.BillingItems.AsNoTracking()
            .ApplyOperationalScope(normalized.ClientId, normalized.From, normalized.To, normalized.Currency)
            .Where(item => item.Status != BillingItemStatus.Cancelled)
            .GroupBy(item => item.DueDate.Date)
            .Select(group => new { Date = group.Key, Amount = group.Sum(item => item.Amount) })
            .ToListAsync(cancellationToken);
        var paymentDays = await ledger.Where(item => item.EntryType == LedgerEntryType.Income)
            .GroupBy(item => item.Date.Date)
            .Select(group => new { Date = group.Key, Amount = group.Sum(item => item.Amount) })
            .ToListAsync(cancellationToken);

        var bucketCount = 6;
        var totalDays = Math.Max(1, (normalized.To - normalized.From).Days + 1);
        var bucketDays = Math.Max(1, (int)Math.Ceiling(totalDays / (decimal)bucketCount));
        for (var start = normalized.From; start <= normalized.To; start = start.AddDays(bucketDays))
        {
            var end = start.AddDays(bucketDays);
            result.Trend.Add(new DashboardTrendPointDto
            {
                PeriodStart = start,
                Label = totalDays <= 14 ? start.ToString("dd MMM", CultureInfo.GetCultureInfo("es-MX")) : $"{start:dd MMM}",
                Charges = chargeDays.Where(item => item.Date >= start && item.Date < end).Sum(item => item.Amount),
                Payments = paymentDays.Where(item => item.Date >= start && item.Date < end).Sum(item => item.Amount)
            });
        }

        var chargesTotal = result.Trend.Sum(item => item.Charges);
        var paymentsTotal = result.Trend.Sum(item => item.Payments);
        result.Summary = paymentsTotal >= chargesTotal
            ? $"Los pagos cubren los cargos del periodo por una diferencia de {(paymentsTotal - chargesTotal):N2} {normalized.Currency}."
            : $"Los cargos superan los pagos del periodo por {(chargesTotal - paymentsTotal):N2} {normalized.Currency}.";
        return result;
    }
}
