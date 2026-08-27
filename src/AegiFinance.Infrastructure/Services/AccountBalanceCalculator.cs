using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
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
        => (await CalculateBalancesAsync(bankAccountId, asOfDate, cancellationToken)).LedgerBalance;

    public async Task<BankAccountBalancesDto> CalculateBalancesAsync(Guid bankAccountId, DateTime? asOfDate = null, CancellationToken cancellationToken = default)
    {
        var account = await _context.BankAccounts
            .AsNoTracking()
            .FirstOrDefaultAsync(ba => ba.Id == bankAccountId, cancellationToken);

        if (account is null)
        {
            throw new InvalidOperationException("La cuenta bancaria no existe.");
        }

        var cutoff = asOfDate?.Date.AddDays(1).AddTicks(-1) ?? DateTime.UtcNow;
        var ledgerAccountId = await _context.GeneralLedgerAccounts
            .AsNoTracking()
            .Where(item => item.BankAccountId == bankAccountId)
            .Select(item => (Guid?)item.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var ledgerBalance = ledgerAccountId.HasValue
            ? await _context.JournalLines.AsNoTracking()
                .Where(line => line.AccountId == ledgerAccountId.Value &&
                    line.JournalEntry.Date <= cutoff &&
                    line.JournalEntry.Status != JournalEntryStatus.Draft)
                .SumAsync(line => line.Debit - line.Credit, cancellationToken)
            : 0m;

        var latestStatement = await _context.BankStatements.AsNoTracking()
            .Where(statement => statement.BankAccountId == bankAccountId && statement.IsBalanceVerified && statement.EndDate <= cutoff)
            .OrderByDescending(statement => statement.EndDate)
            .ThenByDescending(statement => statement.StatementDate)
            .Select(statement => new { statement.ClosingBalance, statement.EndDate })
            .FirstOrDefaultAsync(cancellationToken);

        var bankBalance = latestStatement?.ClosingBalance;
        decimal? comparisonLedgerBalance = latestStatement is null ? null : 0m;
        if (latestStatement is not null && ledgerAccountId.HasValue)
        {
            comparisonLedgerBalance = await _context.JournalLines.AsNoTracking()
                .Where(line => line.AccountId == ledgerAccountId.Value &&
                    line.JournalEntry.Date <= latestStatement.EndDate &&
                    line.JournalEntry.Status != JournalEntryStatus.Draft)
                .SumAsync(line => line.Debit - line.Credit, cancellationToken);
        }
        return new BankAccountBalancesDto(
            account.Id,
            account.Currency,
            cutoff,
            ledgerBalance,
            bankBalance,
            latestStatement?.EndDate,
            comparisonLedgerBalance,
            bankBalance.HasValue ? bankBalance.Value - (comparisonLedgerBalance ?? 0m) : null);
    }
}
