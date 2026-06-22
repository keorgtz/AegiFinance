using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.Subscriptions.Queries.GetSubscriptionPriceHistory;

public class GetSubscriptionPriceHistoryQuery : IRequest<List<SubscriptionPriceHistoryDto>>
{
    public Guid SubscriptionId { get; set; }

    public GetSubscriptionPriceHistoryQuery() { }

    public GetSubscriptionPriceHistoryQuery(Guid subscriptionId)
    {
        SubscriptionId = subscriptionId;
    }
}
