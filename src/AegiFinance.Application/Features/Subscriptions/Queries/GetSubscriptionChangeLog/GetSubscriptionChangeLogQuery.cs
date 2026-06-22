using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.Subscriptions.Queries.GetSubscriptionChangeLog;

public class GetSubscriptionChangeLogQuery : IRequest<List<SubscriptionChangeLogDto>>
{
    public Guid SubscriptionId { get; set; }

    public GetSubscriptionChangeLogQuery() { }

    public GetSubscriptionChangeLogQuery(Guid subscriptionId)
    {
        SubscriptionId = subscriptionId;
    }
}
