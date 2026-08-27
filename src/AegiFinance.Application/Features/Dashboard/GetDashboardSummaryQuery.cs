using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Dashboard;

public sealed class GetDashboardSummaryQuery : IRequest<DashboardSummaryDto>
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public Guid? BankAccountId { get; set; }
    public Guid? ClientId { get; set; }
    public string Currency { get; set; } = "MXN";
}

public sealed class GetDashboardSummaryQueryHandler : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IPermissionService _permissions;

    public GetDashboardSummaryQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser, IPermissionService permissions)
    {
        _context = context;
        _currentUser = currentUser;
        _permissions = permissions;
    }

    public async Task<DashboardSummaryDto> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        var (from, to, clientId, currency) = Normalize(request, _currentUser);
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var permissionCodes = await _permissions.GetEffectivePermissionsAsync(userId, clientId, null, cancellationToken);
        var canViewClients = permissionCodes.Contains("ViewClients", StringComparer.OrdinalIgnoreCase);
        var canViewPayments = permissionCodes.Contains("ViewPayments", StringComparer.OrdinalIgnoreCase);
        var result = new DashboardSummaryDto { From = from, To = to, Currency = currency };

        if (canViewClients)
        {
            var clients = _context.Clients.AsNoTracking().Where(item => item.Status == ClientStatus.Active);
            if (clientId.HasValue) clients = clients.Where(item => item.Id == clientId.Value);
            result.ActiveClients = new DashboardMetricDto { Count = await clients.CountAsync(cancellationToken), Currency = currency };
        }

        if (canViewPayments)
        {
            var charges = _context.BillingItems.AsNoTracking().ApplyOperationalScope(clientId, from, to, currency).Outstanding();
            var outstanding = await charges.Select(item => new { Remaining = item.Amount - item.PaidAmount
                + (item.Adjustments.Where(x => !x.ReversedAt.HasValue && x.Type == BillingAdjustmentType.LateFee).Sum(x => (decimal?)x.Amount) ?? 0m)
                - (item.Adjustments.Where(x => !x.ReversedAt.HasValue && x.Type == BillingAdjustmentType.CreditNote).Sum(x => (decimal?)x.Amount) ?? 0m) }).ToListAsync(cancellationToken);
            var overdue = await charges.Where(item => item.DueDate < DateTime.UtcNow.Date)
                .Select(item => new { Remaining = item.Amount - item.PaidAmount
                    + (item.Adjustments.Where(x => !x.ReversedAt.HasValue && x.Type == BillingAdjustmentType.LateFee).Sum(x => (decimal?)x.Amount) ?? 0m)
                    - (item.Adjustments.Where(x => !x.ReversedAt.HasValue && x.Type == BillingAdjustmentType.CreditNote).Sum(x => (decimal?)x.Amount) ?? 0m) }).ToListAsync(cancellationToken);
            var movements = _context.LedgerEntries.AsNoTracking()
                .ApplyOperationalScope(clientId, request.BankAccountId, from, to, currency);
            result.OutstandingCharges = new DashboardMetricDto { Count = outstanding.Count, Amount = outstanding.Sum(item => item.Remaining), Currency = currency };
            result.OverdueCharges = new DashboardMetricDto { Count = overdue.Count, Amount = overdue.Sum(item => item.Remaining), Currency = currency };
            result.LedgerMovements = new DashboardMetricDto
            {
                Count = await movements.CountAsync(cancellationToken),
                Amount = await movements.SumAsync(item => (decimal?)item.Amount, cancellationToken) ?? 0,
                Currency = currency
            };
        }

        return result;
    }

    internal static (DateTime From, DateTime To, Guid? ClientId, string Currency) Normalize(GetDashboardSummaryQuery request, ICurrentUserService currentUser)
    {
        var to = (request.To ?? DateTime.UtcNow).Date;
        var from = (request.From ?? to.AddDays(-29)).Date;
        if (from > to) throw new InvalidOperationException("La fecha inicial no puede ser posterior a la fecha final.");
        var clientId = string.Equals(currentUser.UserType, "Client", StringComparison.OrdinalIgnoreCase) ? currentUser.ClientId : request.ClientId;
        return (from, to, clientId, string.IsNullOrWhiteSpace(request.Currency) ? "MXN" : request.Currency.Trim().ToUpperInvariant());
    }
}
