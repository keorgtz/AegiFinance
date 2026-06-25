using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using MediatR;

namespace AegiFinance.Application.Features.Allocations.Commands.ManualAllocate;

public class ManualAllocateCommandHandler : IRequestHandler<ManualAllocateCommand, AllocationResult>
{
    private readonly IAllocationService _allocationService;
    private readonly ICurrentUserService _currentUserService;

    public ManualAllocateCommandHandler(IAllocationService allocationService, ICurrentUserService currentUserService)
    {
        _allocationService = allocationService;
        _currentUserService = currentUserService;
    }

    public Task<AllocationResult> Handle(ManualAllocateCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.IsClientUser())
        {
            throw new UnauthorizedAccessException("No tiene permiso para asignar pagos a cargos.");
        }

        return _allocationService.ManualAllocateAsync(request.LedgerEntryId, request.Allocations, cancellationToken);
    }
}
