using MediatR;

namespace AegiFinance.Application.Features.Subscriptions.Commands.DeleteSubscription;

public class DeleteSubscriptionCommand : IRequest
{
    public Guid Id { get; set; }

    public DeleteSubscriptionCommand() { }

    public DeleteSubscriptionCommand(Guid id)
    {
        Id = id;
    }
}
