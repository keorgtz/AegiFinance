using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Common.Models;
using MediatR;

namespace AegiFinance.Application.Features.Billing.Commands.GenerateBilling;

public class GenerateBillingCommandHandler : IRequestHandler<GenerateBillingCommand, BillingGenerationResult>
{
    private readonly IBillingGenerationService _billingGenerationService;
    private readonly ICurrentUserService _currentUserService;

    public GenerateBillingCommandHandler(IBillingGenerationService billingGenerationService, ICurrentUserService currentUserService)
    {
        _billingGenerationService = billingGenerationService;
        _currentUserService = currentUserService;
    }

    public async Task<BillingGenerationResult> Handle(GenerateBillingCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.IsClientUser())
        {
            throw new UnauthorizedAccessException("No tiene permiso para generar facturación.");
        }

        return await _billingGenerationService.GenerateForCycleAsync(
            request.Year,
            request.Month,
            _currentUserService.UserId,
            cancellationToken);
    }
}
