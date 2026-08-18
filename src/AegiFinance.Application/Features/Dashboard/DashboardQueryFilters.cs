using AegiFinance.Domain.Entities;
using AegiFinance.Domain.Enums;

namespace AegiFinance.Application.Features.Dashboard;

public static class DashboardQueryFilters
{
    public static IQueryable<BillingItem> ApplyOperationalScope(
        this IQueryable<BillingItem> query,
        Guid? clientId,
        DateTime? from,
        DateTime? to,
        string? currency)
    {
        if (clientId.HasValue) query = query.Where(item => item.ClientId == clientId.Value);
        if (from.HasValue) query = query.Where(item => item.DueDate >= from.Value);
        if (to.HasValue) query = query.Where(item => item.DueDate < to.Value.Date.AddDays(1));
        if (!string.IsNullOrWhiteSpace(currency)) query = query.Where(item => item.Currency == currency);
        return query;
    }

    public static IQueryable<LedgerEntry> ApplyOperationalScope(
        this IQueryable<LedgerEntry> query,
        Guid? clientId,
        Guid? bankAccountId,
        DateTime? from,
        DateTime? to,
        string? currency)
    {
        if (clientId.HasValue) query = query.Where(item => item.ClientId == clientId.Value);
        if (bankAccountId.HasValue) query = query.Where(item => item.BankAccountId == bankAccountId.Value);
        if (from.HasValue) query = query.Where(item => item.Date >= from.Value);
        if (to.HasValue) query = query.Where(item => item.Date < to.Value.Date.AddDays(1));
        if (!string.IsNullOrWhiteSpace(currency)) query = query.Where(item => item.Currency == currency);
        return query;
    }

    public static IQueryable<BillingItem> Outstanding(this IQueryable<BillingItem> query) =>
        query.Where(item => item.Status == BillingItemStatus.Pending || item.Status == BillingItemStatus.Partial);
}
