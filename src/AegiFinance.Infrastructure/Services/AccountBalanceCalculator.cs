using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Domain.Enums;
using AegiFinance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Infrastructure.Services;

public class AccountBalanceCalculator : IAccountBalanceCalculator
{
    private readonly ApplicationDbContext _context;

    public AccountBalanceCalculator(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<decimal> CalculateBalanceAsync(Guid bankAccountId, DateTime? asOfDate = null, CancellationToken cancellationToken = default)
    {
        var account = await _context.BankAccounts
            .AsNoTracking()
            .FirstOrDefaultAsync(ba => ba.Id == bankAccountId, cancellationToken);

        if (account is null)
        {
            throw new InvalidOperationException("La cuenta bancaria no existe.");
        }

        var query = _context.LedgerEntries
            .AsNoTracking()
            .Where(le => le.BankAccountId == bankAccountId);

        if (asOfDate.HasValue)
        {
            var endDate = asOfDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(le => le.Date <= endDate);
        }

        var entries = await query.ToListAsync(cancellationToken);

        var balance = account.OpeningBalance
            + entries.Where(e => e.EntryType == LedgerEntryType.Income).Sum(e => e.Amount)
            - entries.Where(e => e.EntryType == LedgerEntryType.Expense).Sum(e => e.Amount)
            + entries.Where(e => e.EntryType == LedgerEntryType.TransferIn).Sum(e => e.Amount)
            - entries.Where(e => e.EntryType == LedgerEntryType.TransferOut).Sum(e => e.Amount)
            + entries.Where(e => e.EntryType == LedgerEntryType.Adjustment).Sum(e => e.Amount);

        return balance;
    }
}
