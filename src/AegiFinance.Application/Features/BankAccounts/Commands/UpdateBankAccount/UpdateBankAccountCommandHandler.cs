using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.BankAccounts.Commands.UpdateBankAccount;

public class UpdateBankAccountCommandHandler : IRequestHandler<UpdateBankAccountCommand, BankAccountDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateBankAccountCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BankAccountDto> Handle(UpdateBankAccountCommand request, CancellationToken cancellationToken)
    {
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
        account.AccountNumber = request.AccountNumber;
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
            AccountNumber = account.AccountNumber,
            Currency = account.Currency,
            OpeningBalance = account.OpeningBalance,
            OpeningDate = account.OpeningDate,
            IsActive = account.IsActive
        };
    }
}
