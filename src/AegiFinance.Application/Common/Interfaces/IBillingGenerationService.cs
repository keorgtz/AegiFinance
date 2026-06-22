using AegiFinance.Application.Common.Models;

namespace AegiFinance.Application.Common.Interfaces;

public interface IBillingGenerationService
{
    Task<BillingGenerationResult> GenerateForCycleAsync(int year, int? month, Guid? triggeredBy = null, CancellationToken cancellationToken = default);
    Task<BillingGenerationResult> GenerateForSubscriptionAsync(Guid subscriptionId, Guid? triggeredBy = null, CancellationToken cancellationToken = default);
    Task<BillingGenerationResult> ReprocessCycleAsync(Guid billingCycleId, bool onlyPending = true, Guid? triggeredBy = null, CancellationToken cancellationToken = default);
}
