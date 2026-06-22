using AegiFinance.Application.Common.Models;
using MediatR;

namespace AegiFinance.Application.Features.Billing.Commands.GenerateBillingForSubscription;

public class GenerateBillingForSubscriptionCommand : IRequest<BillingGenerationResult>
{
    public Guid SubscriptionId { get; set; }
}
