using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.SubscriptionPermissions.Queries.GetSubscriptionPermissionsBySubscription;

public class GetSubscriptionPermissionsBySubscriptionQuery : IRequest<List<SubscriptionPermissionDto>>
{
    public Guid SubscriptionId { get; set; }

    public GetSubscriptionPermissionsBySubscriptionQuery() { }

    public GetSubscriptionPermissionsBySubscriptionQuery(Guid subscriptionId)
    {
        SubscriptionId = subscriptionId;
    }
}
