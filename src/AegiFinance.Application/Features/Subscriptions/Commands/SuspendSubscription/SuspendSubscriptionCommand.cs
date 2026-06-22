using MediatR;

namespace AegiFinance.Application.Features.Subscriptions.Commands.SuspendSubscription;

public class SuspendSubscriptionCommand : IRequest
{
    public Guid Id { get; set; }
    public string? Reason { get; set; }

    public SuspendSubscriptionCommand() { }

    public SuspendSubscriptionCommand(Guid id, string? reason = null)
    {
        Id = id;
        Reason = reason;
    }
}
