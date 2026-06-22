using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.BankAccounts.Commands.CreateBankAccount;

public class CreateBankAccountCommandHandler : IRequestHandler<CreateBankAccountCommand, BankAccountDto>
{
    private readonly IApplicationDbContext _context;

    public CreateBankAccountCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BankAccountDto> Handle(CreateBankAccountCommand request, CancellationToken cancellationToken)
    {
        var nameExists = await _context.BankAccounts
            .AsNoTracking()
            .AnyAsync(ba => ba.Name == request.Name, cancellationToken);

        if (nameExists)
        {
            throw new InvalidOperationException("Ya existe una cuenta bancaria con ese nombre.");
        }

        var account = new BankAccount
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            BankName = request.BankName,
            AccountNumber = request.AccountNumber,
            Currency = request.Currency,
            OpeningBalance = request.OpeningBalance,
            OpeningDate = request.OpeningDate,
            IsActive = request.IsActive
        };

        _context.BankAccounts.Add(account);
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
