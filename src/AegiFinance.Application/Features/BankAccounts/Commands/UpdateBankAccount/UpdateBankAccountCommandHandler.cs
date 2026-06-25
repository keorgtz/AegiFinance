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

    public UpdateBankAccountCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService, IAccountNumberProtector accountNumberProtector)
    {
        _context = context;
        _currentUserService = currentUserService;
        _accountNumberProtector = accountNumberProtector;
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

        account.Name = request.Name;
        account.BankName = request.BankName;
        account.AccountNumber = string.IsNullOrWhiteSpace(request.AccountNumber) ? null : _accountNumberProtector.Protect(request.AccountNumber);
        account.Currency = request.Currency;
        account.OpeningBalance = request.OpeningBalance;
        account.OpeningDate = request.OpeningDate;
        account.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        return new BankAccountDto
        {
            Id = account.Id,
            Name = account.Name,
            BankName = account.BankName,
            MaskedAccountNumber = _accountNumberProtector.MaskFromProtected(account.AccountNumber),
            Currency = account.Currency,
            OpeningBalance = account.OpeningBalance,
            OpeningDate = account.OpeningDate,
            IsActive = account.IsActive
        };
    }
}
