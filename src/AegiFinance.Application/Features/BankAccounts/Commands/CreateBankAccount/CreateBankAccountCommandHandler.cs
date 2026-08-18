using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.BankAccounts.Commands.CreateBankAccount;

public class CreateBankAccountCommandHandler : IRequestHandler<CreateBankAccountCommand, BankAccountDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAccountNumberProtector _accountNumberProtector;
    private readonly IMajorLedgerService _majorLedger;
    private readonly IAccountBalanceCalculator _balanceCalculator;

    public CreateBankAccountCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService,
        IAccountNumberProtector accountNumberProtector, IMajorLedgerService majorLedger,
        IAccountBalanceCalculator balanceCalculator)
    {
        _context = context;
        _currentUserService = currentUserService;
        _accountNumberProtector = accountNumberProtector;
        _majorLedger = majorLedger;
        _balanceCalculator = balanceCalculator;
    }

    public async Task<BankAccountDto> Handle(CreateBankAccountCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.IsClientUser())
        {
            throw new UnauthorizedAccessException("No tiene permiso para crear cuentas bancarias.");
        }

        var nameExists = await _context.BankAccounts
            .AsNoTracking()
            .AnyAsync(ba => ba.Name == request.Name, cancellationToken);

        if (nameExists)
        {
            throw new InvalidOperationException("Ya existe una cuenta bancaria con ese nombre.");
        }

        await using var transaction = await _context.BeginTransactionAsync(cancellationToken);
        var organizationId = _currentUserService.OrganizationId
            ?? throw new InvalidOperationException("El usuario no tiene una organización asignada.");
        var account = new BankAccount
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            Name = request.Name,
            BankName = request.BankName,
            AccountNumber = string.IsNullOrWhiteSpace(request.AccountNumber) ? null : _accountNumberProtector.Protect(request.AccountNumber),
            Currency = request.Currency.ToUpperInvariant(),
            OpeningBalance = request.OpeningBalance,
            OpeningDate = request.OpeningDate,
            IsActive = request.IsActive
        };

        _context.BankAccounts.Add(account);
        await _context.SaveChangesAsync(cancellationToken);
        await _majorLedger.EnsureBaseChartAsync(cancellationToken);
        await _majorLedger.PostOpeningBalanceAsync(account, cancellationToken);
        await transaction.CommitAsync(cancellationToken);

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
