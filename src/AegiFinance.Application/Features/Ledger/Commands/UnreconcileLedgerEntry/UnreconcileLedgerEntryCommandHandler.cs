using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using MediatR;

namespace AegiFinance.Application.Features.Ledger.Commands.UnreconcileLedgerEntry;

public class UnreconcileLedgerEntryCommandHandler : IRequestHandler<UnreconcileLedgerEntryCommand>
{
    private readonly ILedgerService _ledgerService;
    private readonly ICurrentUserService _currentUserService;

    public UnreconcileLedgerEntryCommandHandler(ILedgerService ledgerService, ICurrentUserService currentUserService)
    {
        _ledgerService = ledgerService;
        _currentUserService = currentUserService;
    }

    public async Task Handle(UnreconcileLedgerEntryCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.IsClientUser())
        {
            throw new UnauthorizedAccessException("No tiene permiso para deshacer la reconciliación de movimientos.");
        }

        await _ledgerService.UnreconcileAsync(request.LedgerEntryId, cancellationToken);
    }
}
