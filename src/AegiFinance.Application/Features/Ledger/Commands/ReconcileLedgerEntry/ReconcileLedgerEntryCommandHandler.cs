using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using MediatR;

namespace AegiFinance.Application.Features.Ledger.Commands.ReconcileLedgerEntry;

public class ReconcileLedgerEntryCommandHandler : IRequestHandler<ReconcileLedgerEntryCommand>
{
    private readonly ILedgerService _ledgerService;
    private readonly ICurrentUserService _currentUserService;

    public ReconcileLedgerEntryCommandHandler(ILedgerService ledgerService, ICurrentUserService currentUserService)
    {
        _ledgerService = ledgerService;
        _currentUserService = currentUserService;
    }

    public async Task Handle(ReconcileLedgerEntryCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.IsClientUser())
        {
            throw new UnauthorizedAccessException("No tiene permiso para reconciliar movimientos.");
        }

        await _ledgerService.ReconcileAsync(request.LedgerEntryId, cancellationToken);
    }
}
