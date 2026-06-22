using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Common.Models;
using MediatR;

namespace AegiFinance.Application.Features.Billing.Commands.ReprocessBillingCycle;

public class ReprocessBillingCycleCommandHandler : IRequestHandler<ReprocessBillingCycleCommand, BillingGenerationResult>
{
    private readonly IBillingGenerationService _billingGenerationService;
    private readonly ICurrentUserService _currentUserService;

    public ReprocessBillingCycleCommandHandler(IBillingGenerationService billingGenerationService, ICurrentUserService currentUserService)
    {
        _billingGenerationService = billingGenerationService;
        _currentUserService = currentUserService;
    }

    public async Task<BillingGenerationResult> Handle(ReprocessBillingCycleCommand request, CancellationToken cancellationToken)
    {
        return await _billingGenerationService.ReprocessCycleAsync(
            request.BillingCycleId,
            request.OnlyPending,
            _currentUserService.UserId,
            cancellationToken);
    }
}
