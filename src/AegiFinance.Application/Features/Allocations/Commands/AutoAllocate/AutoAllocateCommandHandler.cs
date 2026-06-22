using AegiFinance.Application.Common.Interfaces;
using MediatR;

namespace AegiFinance.Application.Features.Allocations.Commands.AutoAllocate;

public class AutoAllocateCommandHandler : IRequestHandler<AutoAllocateCommand, AllocationResult>
{
    private readonly IAllocationService _allocationService;

    public AutoAllocateCommandHandler(IAllocationService allocationService)
    {
        _allocationService = allocationService;
    }

    public Task<AllocationResult> Handle(AutoAllocateCommand request, CancellationToken cancellationToken)
        => _allocationService.AutoAllocateAsync(request.LedgerEntryId, cancellationToken);
}
