using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Common.Models;
using MediatR;

namespace AegiFinance.Application.Features.Billing.Commands.GenerateBillingForSubscription;

public class GenerateBillingForSubscriptionCommandHandler : IRequestHandler<GenerateBillingForSubscriptionCommand, BillingGenerationResult>
{
    private readonly IBillingGenerationService _billingGenerationService;
    private readonly ICurrentUserService _currentUserService;

    public GenerateBillingForSubscriptionCommandHandler(IBillingGenerationService billingGenerationService, ICurrentUserService currentUserService)
    {
        _billingGenerationService = billingGenerationService;
        _currentUserService = currentUserService;
    }

    public async Task<BillingGenerationResult> Handle(GenerateBillingForSubscriptionCommand request, CancellationToken cancellationToken)
    {
        return await _billingGenerationService.GenerateForSubscriptionAsync(
            request.SubscriptionId,
            _currentUserService.UserId,
            cancellationToken);
    }
}
