using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.Subscriptions.Queries.GetSubscriptionById;

public class GetSubscriptionByIdQuery : IRequest<SubscriptionDetailDto>
{
    public Guid Id { get; set; }

    public GetSubscriptionByIdQuery() { }

    public GetSubscriptionByIdQuery(Guid id)
    {
        Id = id;
    }
}
