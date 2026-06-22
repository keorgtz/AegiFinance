using AegiFinance.Application.Common.Interfaces;
using MediatR;

namespace AegiFinance.Application.Features.Ledger.Commands.ReconcileLedgerEntry;

public class ReconcileLedgerEntryCommandHandler : IRequestHandler<ReconcileLedgerEntryCommand>
{
    private readonly ILedgerService _ledgerService;

    public ReconcileLedgerEntryCommandHandler(ILedgerService ledgerService)
    {
        _ledgerService = ledgerService;
    }

    public async Task Handle(ReconcileLedgerEntryCommand request, CancellationToken cancellationToken)
    {
        await _ledgerService.ReconcileAsync(request.LedgerEntryId, cancellationToken);
    }
}
