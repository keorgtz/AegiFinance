using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.BankAccounts.Commands.DeleteBankAccount;

public class DeleteBankAccountCommandHandler : IRequestHandler<DeleteBankAccountCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeleteBankAccountCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(DeleteBankAccountCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.IsClientUser())
        {
            throw new UnauthorizedAccessException("No tiene permiso para eliminar cuentas bancarias.");
        }

        var account = await _context.BankAccounts
            .FirstOrDefaultAsync(ba => ba.Id == request.Id, cancellationToken);

        if (account is null)
        {
            throw new InvalidOperationException("La cuenta bancaria no existe.");
        }

        var hasMovements = await _context.LedgerEntries
            .AsNoTracking()
            .AnyAsync(le => le.BankAccountId == request.Id, cancellationToken);

        if (hasMovements)
        {
            throw new InvalidOperationException("No se puede eliminar una cuenta con movimientos registrados.");
        }

        _context.BankAccounts.Remove(account);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
