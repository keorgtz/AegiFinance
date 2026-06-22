using AegiFinance.Application.Common.Models;
using MediatR;

namespace AegiFinance.Application.Features.Billing.Commands.ReprocessBillingCycle;

public class ReprocessBillingCycleCommand : IRequest<BillingGenerationResult>
{
    public Guid BillingCycleId { get; set; }
    public bool OnlyPending { get; set; } = true;
}
