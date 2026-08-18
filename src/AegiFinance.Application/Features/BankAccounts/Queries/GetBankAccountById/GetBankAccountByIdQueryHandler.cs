using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.BankAccounts.Queries.GetBankAccountById;

public class GetBankAccountByIdQueryHandler : IRequestHandler<GetBankAccountByIdQuery, BankAccountDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAccountNumberProtector _accountNumberProtector;
    private readonly IAccountBalanceCalculator _balanceCalculator;

    public GetBankAccountByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService,
        IAccountNumberProtector accountNumberProtector, IAccountBalanceCalculator balanceCalculator)
    {
        _context = context;
        _currentUserService = currentUserService;
        _accountNumberProtector = accountNumberProtector;
        _balanceCalculator = balanceCalculator;
    }

    public async Task<BankAccountDto> Handle(GetBankAccountByIdQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.IsClientUser())
        {
            throw new UnauthorizedAccessException("No tiene permiso para consultar cuentas bancarias.");
        }

        var account = await _context.BankAccounts
            .AsNoTracking()
            .FirstOrDefaultAsync(ba => ba.Id == request.Id, cancellationToken);

        if (account is null)
        {
            throw new InvalidOperationException("La cuenta bancaria no existe.");
        }

        var balances = await _balanceCalculator.CalculateBalancesAsync(account.Id, null, cancellationToken);
        return new BankAccountDto
        {
            Id = account.Id,
            Name = account.Name,
            BankName = account.BankName,
            MaskedAccountNumber = _accountNumberProtector.MaskFromProtected(account.AccountNumber),
            Currency = account.Currency,
            OpeningBalance = account.OpeningBalance,
            OpeningDate = account.OpeningDate,
            LedgerBalance = balances.LedgerBalance,
            BankBalance = balances.BankBalance,
            BankBalanceAsOfDate = balances.BankBalanceAsOfDate,
            ComparisonLedgerBalance = balances.ComparisonLedgerBalance,
            Difference = balances.Difference,
            IsActive = account.IsActive
        };
    }
}
