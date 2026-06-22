using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.Allocations.Queries.GetClientAllocations;

public class GetClientAllocationsQuery : IRequest<List<SubscriptionAllocationDto>>
{
    public Guid ClientId { get; set; }

    public GetClientAllocationsQuery() { }

    public GetClientAllocationsQuery(Guid clientId)
    {
        ClientId = clientId;
    }
}
