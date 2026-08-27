using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Billing.Queries.GetReceivablesAging;

public class GetReceivablesAgingQueryHandler : IRequestHandler<GetReceivablesAgingQuery, ReceivablesAgingDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public GetReceivablesAgingQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser) => (_context, _currentUser) = (context, currentUser);
    public async Task<ReceivablesAgingDto> Handle(GetReceivablesAgingQuery request, CancellationToken cancellationToken)
    {
        var asOf = (request.AsOfDate ?? DateTime.UtcNow).Date;
        var currency = request.Currency.Trim().ToUpperInvariant();
        if (_currentUser.IsClientUser() && !_currentUser.ClientId.HasValue)
            return new ReceivablesAgingDto(asOf, currency, 0m, 0m, 0m, true, new[] { new AgingBucketDto("current", "Por vencer", 0, 0m),
                new AgingBucketDto("1-30", "1–30 días", 0, 0m), new AgingBucketDto("31-60", "31–60 días", 0, 0m),
                new AgingBucketDto("61-90", "61–90 días", 0, 0m), new AgingBucketDto("90+", "Más de 90 días", 0, 0m) });
        var clientId = _currentUser.IsClientUser() ? _currentUser.ClientId : request.ClientId;
        var rows = await _context.BillingItems.AsNoTracking()
            .Where(x => x.Status != BillingItemStatus.Cancelled && x.Status != BillingItemStatus.Paid && x.Currency == currency && (!clientId.HasValue || x.ClientId == clientId))
            .Select(x => new { x.DueDate, Balance = x.Amount - x.PaidAmount
                + (x.Adjustments.Where(a => !a.ReversedAt.HasValue && a.Type == BillingAdjustmentType.LateFee).Sum(a => (decimal?)a.Amount) ?? 0m)
                - (x.Adjustments.Where(a => !a.ReversedAt.HasValue && a.Type == BillingAdjustmentType.CreditNote).Sum(a => (decimal?)a.Amount) ?? 0m) })
            .Where(x => x.Balance > 0).ToListAsync(cancellationToken);
        var definitions = new[] { ("current", "Por vencer", int.MinValue, -1), ("1-30", "1–30 días", 0, 29),
            ("31-60", "31–60 días", 30, 59), ("61-90", "61–90 días", 60, 89), ("90+", "Más de 90 días", 90, int.MaxValue) };
        var buckets = definitions.Select(def => { var matches = rows.Where(x => { var days = (asOf - x.DueDate.Date).Days; return days >= def.Item3 && days <= def.Item4; }).ToList();
            return new AgingBucketDto(def.Item1, def.Item2, matches.Count, matches.Sum(x => x.Balance)); }).ToList();
        var total = rows.Sum(x => x.Balance);
        var cutoff = asOf.AddDays(1);
        var ledgerBalance = await _context.JournalLines.AsNoTracking()
            .Where(line => line.Account.Purpose == GeneralLedgerAccountPurpose.AccountsReceivable && line.Account.Currency == currency
                && line.JournalEntry.Status != JournalEntryStatus.Draft && line.JournalEntry.Date < cutoff
                && (!clientId.HasValue || line.ClientId == clientId))
            .SumAsync(line => (decimal?)(line.Debit - line.Credit), cancellationToken) ?? 0m;
        var difference = total - ledgerBalance;
        return new ReceivablesAgingDto(asOf, currency, total, ledgerBalance, difference, Math.Abs(difference) < 0.01m, buckets);
    }
}
