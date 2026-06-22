using AegiFinance.Application.Common.Interfaces;
using MediatR;

namespace AegiFinance.Application.Features.Allocations.Commands.ManualAllocate;

public class ManualAllocateCommand : IRequest<AllocationResult>
{
    public Guid LedgerEntryId { get; set; }
    public List<ManualAllocationRequest> Allocations { get; set; } = new();
}
