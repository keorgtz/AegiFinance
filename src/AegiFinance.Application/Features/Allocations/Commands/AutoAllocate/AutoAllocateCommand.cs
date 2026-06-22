using AegiFinance.Application.Common.Interfaces;
using MediatR;

namespace AegiFinance.Application.Features.Allocations.Commands.AutoAllocate;

public class AutoAllocateCommand : IRequest<AllocationResult>
{
    public Guid LedgerEntryId { get; set; }

    public AutoAllocateCommand() { }

    public AutoAllocateCommand(Guid ledgerEntryId)
    {
        LedgerEntryId = ledgerEntryId;
    }
}
