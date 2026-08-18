using MediatR;

namespace AegiFinance.Application.Features.Subscriptions.Commands.RenewSubscription;

public class RenewSubscriptionCommand : IRequest
{
    public Guid Id { get; set; }
    public string? Reason { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;

    public RenewSubscriptionCommand() { }

    public RenewSubscriptionCommand(Guid id, string? reason = null)
    {
        Id = id;
        Reason = reason;
    }
}
