using AegiFinance.Application.Common.Extensions;
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
        if (_currentUserService.IsClientUser())
        {
            throw new UnauthorizedAccessException("No tiene permiso para reprocesar la facturación.");
        }

        return await _billingGenerationService.ReprocessCycleAsync(
            request.BillingCycleId,
            request.OnlyPending,
            _currentUserService.UserId,
            cancellationToken);
    }
}
