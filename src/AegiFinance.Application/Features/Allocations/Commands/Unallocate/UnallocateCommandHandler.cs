using AegiFinance.Application.Common.Interfaces;
using MediatR;

namespace AegiFinance.Application.Features.Allocations.Commands.Unallocate;

public class UnallocateCommandHandler : IRequestHandler<UnallocateCommand>
{
    private readonly IAllocationService _allocationService;

    public UnallocateCommandHandler(IAllocationService allocationService)
    {
        _allocationService = allocationService;
    }

    public async Task Handle(UnallocateCommand request, CancellationToken cancellationToken)
    {
        await _allocationService.UnallocateAsync(request.AllocationId, cancellationToken);
    }
}
