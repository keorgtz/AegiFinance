using AegiFinance.Application.Common.Interfaces;
using MediatR;

namespace AegiFinance.Application.Features.Allocations.Commands.ManualAllocate;

public class ManualAllocateCommandHandler : IRequestHandler<ManualAllocateCommand, AllocationResult>
{
    private readonly IAllocationService _allocationService;

    public ManualAllocateCommandHandler(IAllocationService allocationService)
    {
        _allocationService = allocationService;
    }

    public Task<AllocationResult> Handle(ManualAllocateCommand request, CancellationToken cancellationToken)
        => _allocationService.ManualAllocateAsync(request.LedgerEntryId, request.Allocations, cancellationToken);
}
