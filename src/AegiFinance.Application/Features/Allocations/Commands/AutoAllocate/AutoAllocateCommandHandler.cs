using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using MediatR;

namespace AegiFinance.Application.Features.Allocations.Commands.AutoAllocate;

public class AutoAllocateCommandHandler : IRequestHandler<AutoAllocateCommand, AllocationResult>
{
    private readonly IAllocationService _allocationService;
    private readonly ICurrentUserService _currentUserService;

    public AutoAllocateCommandHandler(IAllocationService allocationService, ICurrentUserService currentUserService)
    {
        _allocationService = allocationService;
        _currentUserService = currentUserService;
    }

    public Task<AllocationResult> Handle(AutoAllocateCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.IsClientUser())
        {
            throw new UnauthorizedAccessException("No tiene permiso para asignar pagos a cargos.");
        }

        return _allocationService.AutoAllocateAsync(request.LedgerEntryId, cancellationToken);
    }
}
