using MediatR;

namespace AegiFinance.Application.Features.Billing.Commands.CloseBillingCycle;

public class CloseBillingCycleCommand : IRequest
{
    public Guid BillingCycleId { get; set; }
}
