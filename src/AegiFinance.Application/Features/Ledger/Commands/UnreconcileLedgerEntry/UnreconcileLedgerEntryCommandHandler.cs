using AegiFinance.Application.Common.Interfaces;
using MediatR;

namespace AegiFinance.Application.Features.Ledger.Commands.UnreconcileLedgerEntry;

public class UnreconcileLedgerEntryCommandHandler : IRequestHandler<UnreconcileLedgerEntryCommand>
{
    private readonly ILedgerService _ledgerService;

    public UnreconcileLedgerEntryCommandHandler(ILedgerService ledgerService)
    {
        _ledgerService = ledgerService;
    }

    public async Task Handle(UnreconcileLedgerEntryCommand request, CancellationToken cancellationToken)
    {
        await _ledgerService.UnreconcileAsync(request.LedgerEntryId, cancellationToken);
    }
}
