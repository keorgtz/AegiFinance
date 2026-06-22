using MediatR;

namespace AegiFinance.Application.Features.Subscriptions.Commands.ReactivateSubscription;

public class ReactivateSubscriptionCommand : IRequest
{
    public Guid Id { get; set; }
    public string? Reason { get; set; }

    public ReactivateSubscriptionCommand() { }

    public ReactivateSubscriptionCommand(Guid id, string? reason = null)
    {
        Id = id;
        Reason = reason;
    }
}
