using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.BankAccounts.Commands.UpdateBankAccount;

public class UpdateBankAccountCommandHandler : IRequestHandler<UpdateBankAccountCommand, BankAccountDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAccountNumberProtector _accountNumberProtector;
    private readonly IAccountBalanceCalculator _balanceCalculator;

    public UpdateBankAccountCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService,
        IAccountNumberProtector accountNumberProtector, IAccountBalanceCalculator balanceCalculator)
    {
        _context = context;
        _currentUserService = currentUserService;
        _accountNumberProtector = accountNumberProtector;
        _balanceCalculator = balanceCalculator;
    }

    public async Task<BankAccountDto> Handle(UpdateBankAccountCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.IsClientUser())
        {
            throw new UnauthorizedAccessException("No tiene permiso para modificar cuentas bancarias.");
        }

        var account = await _context.BankAccounts
            .FirstOrDefaultAsync(ba => ba.Id == request.Id, cancellationToken);

        if (account is null)
        {
            throw new InvalidOperationException("La cuenta bancaria no existe.");
        }

        var nameExists = await _context.BankAccounts
            .AsNoTracking()
            .AnyAsync(ba => ba.Id != request.Id && ba.Name == request.Name, cancellationToken);

        if (nameExists)
        {
            throw new InvalidOperationException("Ya existe una cuenta bancaria con ese nombre.");
        }

        if (!string.Equals(account.Currency, request.Currency, StringComparison.OrdinalIgnoreCase) ||
            account.OpeningBalance != request.OpeningBalance ||
            account.OpeningDate.Date != request.OpeningDate.Date)
        {
            throw new InvalidOperationException("La moneda, el saldo inicial y la fecha de apertura son inmutables. Registre un ajuste contable si necesita corregir el saldo.");
        }

        account.Name = request.Name;
        account.BankName = request.BankName;
        var currentMask = _accountNumberProtector.MaskFromProtected(account.AccountNumber);
        if (!string.Equals(currentMask, request.AccountNumber, StringComparison.Ordinal))
        {
            account.AccountNumber = string.IsNullOrWhiteSpace(request.AccountNumber) ? null : _accountNumberProtector.Protect(request.AccountNumber);
        }
        account.IsActive = request.IsActive;

        var ledgerAccount = await _context.GeneralLedgerAccounts
            .FirstOrDefaultAsync(item => item.BankAccountId == account.Id, cancellationToken);
        if (ledgerAccount is not null)
        {
            ledgerAccount.Name = $"Bank · {account.Name}";
            ledgerAccount.IsActive = account.IsActive;
        }

        await _context.SaveChangesAsync(cancellationToken);
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
