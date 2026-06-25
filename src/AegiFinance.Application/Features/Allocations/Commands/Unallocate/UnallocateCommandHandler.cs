using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using MediatR;

namespace AegiFinance.Application.Features.Allocations.Commands.Unallocate;

public class UnallocateCommandHandler : IRequestHandler<UnallocateCommand>
{
    private readonly IAllocationService _allocationService;
    private readonly ICurrentUserService _currentUserService;

    public UnallocateCommandHandler(IAllocationService allocationService, ICurrentUserService currentUserService)
    {
        _allocationService = allocationService;
        _currentUserService = currentUserService;
    }

    public async Task Handle(UnallocateCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.IsClientUser())
        {
            throw new UnauthorizedAccessException("No tiene permiso para desasignar pagos de cargos.");
        }

        await _allocationService.UnallocateAsync(request.AllocationId, cancellationToken);
    }
}
