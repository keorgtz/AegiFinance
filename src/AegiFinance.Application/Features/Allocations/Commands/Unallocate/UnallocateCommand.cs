using MediatR;

namespace AegiFinance.Application.Features.Allocations.Commands.Unallocate;

public class UnallocateCommand : IRequest
{
    public Guid AllocationId { get; set; }

    public UnallocateCommand() { }

    public UnallocateCommand(Guid allocationId)
    {
        AllocationId = allocationId;
    }
}
