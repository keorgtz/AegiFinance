using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.Subscriptions.Queries.GetClientSubscriptions;

public class GetClientSubscriptionsQuery : IRequest<List<SubscriptionListDto>>
{
    public Guid ClientId { get; set; }

    public GetClientSubscriptionsQuery() { }

    public GetClientSubscriptionsQuery(Guid clientId)
    {
        ClientId = clientId;
    }
}
