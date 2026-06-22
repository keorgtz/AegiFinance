using MediatR;

namespace AegiFinance.Application.Features.Ledger.Commands.ReconcileLedgerEntry;

public class ReconcileLedgerEntryCommand : IRequest
{
    public Guid LedgerEntryId { get; set; }

    public ReconcileLedgerEntryCommand() { }

    public ReconcileLedgerEntryCommand(Guid ledgerEntryId)
    {
        LedgerEntryId = ledgerEntryId;
    }
}
