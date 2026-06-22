using MediatR;

namespace AegiFinance.Application.Features.Subscriptions.Commands.CancelSubscription;

public class CancelSubscriptionCommand : IRequest
{
    public Guid Id { get; set; }
    public string? Reason { get; set; }

    public CancelSubscriptionCommand() { }

    public CancelSubscriptionCommand(Guid id, string? reason = null)
    {
        Id = id;
        Reason = reason;
    }
}
