using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Common.Models;
using AegiFinance.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;
using AegiFinance.Domain.Enums;

namespace AegiFinance.Application.Features.BankAccounts.Queries.GetBankAccounts;

public class GetBankAccountsQueryHandler : IRequestHandler<GetBankAccountsQuery, PaginatedList<BankAccountListDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAccountNumberProtector _accountNumberProtector;

    public GetBankAccountsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService,
        IAccountNumberProtector accountNumberProtector)
    {
        _context = context;
        _currentUserService = currentUserService;
        _accountNumberProtector = accountNumberProtector;
    }

    public async Task<PaginatedList<BankAccountListDto>> Handle(GetBankAccountsQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.IsClientUser())
        {
            throw new UnauthorizedAccessException("No tiene permiso para consultar cuentas bancarias.");
        }

        var query = _context.BankAccounts
            .AsNoTracking()
            .AsQueryable();

        if (request.IsActive.HasValue)
        {
            query = query.Where(ba => ba.IsActive == request.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(ba =>
                EF.Functions.Like(ba.Name.ToLower(), $"%{search}%") ||
                (ba.BankName != null && EF.Functions.Like(ba.BankName.ToLower(), $"%{search}%")));
        }

        query = query.OrderBy(ba => ba.Name);

        var projected = query.Select(ba => new BankAccountListDto
        {
            Id = ba.Id,
            Name = ba.Name,
            BankName = ba.BankName,
            MaskedAccountNumber = ba.AccountNumber,
            Currency = ba.Currency,
            OpeningBalance = ba.OpeningBalance,
            OpeningDate = ba.OpeningDate,
            IsActive = ba.IsActive
        });

        var result = await projected.ToPaginatedListAsync(request.PageNumber, request.PageSize, cancellationToken);

        foreach (var item in result.Items)
            item.MaskedAccountNumber = _accountNumberProtector.MaskFromProtected(item.MaskedAccountNumber);
        if (!request.IncludeBalances || result.Items.Count == 0) return result;

        var accountIds = result.Items.Select(item => item.Id).ToList();
        var ledgerBalances = await _context.JournalLines.AsNoTracking()
            .Where(line => line.BankAccountId.HasValue && accountIds.Contains(line.BankAccountId.Value) &&
                line.JournalEntry.Status != JournalEntryStatus.Draft)
            .GroupBy(line => line.BankAccountId!.Value)
            .Select(group => new { BankAccountId = group.Key, Balance = group.Sum(line => line.Debit - line.Credit) })
            .ToDictionaryAsync(item => item.BankAccountId, item => item.Balance, cancellationToken);
        var statements = await _context.BankStatements.AsNoTracking()
            .Where(statement => accountIds.Contains(statement.BankAccountId) && statement.IsBalanceVerified && statement.EndDate <= DateTime.UtcNow)
            .OrderByDescending(statement => statement.EndDate)
            .ThenByDescending(statement => statement.StatementDate)
            .Select(statement => new { statement.BankAccountId, statement.ClosingBalance, statement.EndDate })
            .ToListAsync(cancellationToken);
        var latestStatements = statements.GroupBy(statement => statement.BankAccountId)
            .ToDictionary(group => group.Key, group => group.First());

        foreach (var item in result.Items)
        {
            item.LedgerBalance = ledgerBalances.GetValueOrDefault(item.Id);
            if (latestStatements.TryGetValue(item.Id, out var statement))
            {
                var comparison = await _context.JournalLines.AsNoTracking()
                    .Where(line => line.BankAccountId == item.Id && line.JournalEntry.Date <= statement.EndDate &&
                        line.JournalEntry.Status != JournalEntryStatus.Draft)
                    .SumAsync(line => line.Debit - line.Credit, cancellationToken);
                item.BankBalance = statement.ClosingBalance;
                item.BankBalanceAsOfDate = statement.EndDate;
                item.ComparisonLedgerBalance = comparison;
                item.Difference = statement.ClosingBalance - comparison;
            }
        }

        return result;
    }
}
