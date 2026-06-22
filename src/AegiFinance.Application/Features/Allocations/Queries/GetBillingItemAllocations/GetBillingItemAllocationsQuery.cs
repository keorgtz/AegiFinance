using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.Allocations.Queries.GetBillingItemAllocations;

public class GetBillingItemAllocationsQuery : IRequest<List<SubscriptionAllocationDto>>
{
    public Guid BillingItemId { get; set; }

    public GetBillingItemAllocationsQuery() { }

    public GetBillingItemAllocationsQuery(Guid billingItemId)
    {
        BillingItemId = billingItemId;
    }
}
