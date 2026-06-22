using MediatR;

namespace AegiFinance.Application.Features.Ledger.Commands.UnreconcileLedgerEntry;

public class UnreconcileLedgerEntryCommand : IRequest
{
    public Guid LedgerEntryId { get; set; }

    public UnreconcileLedgerEntryCommand() { }

    public UnreconcileLedgerEntryCommand(Guid ledgerEntryId)
    {
        LedgerEntryId = ledgerEntryId;
    }
}
