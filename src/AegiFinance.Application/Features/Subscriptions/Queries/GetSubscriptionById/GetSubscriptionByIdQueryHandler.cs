using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Subscriptions.Queries.GetSubscriptionById;

public class GetSubscriptionByIdQueryHandler : IRequestHandler<GetSubscriptionByIdQuery, SubscriptionDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetSubscriptionByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<SubscriptionDetailDto> Handle(GetSubscriptionByIdQuery request, CancellationToken cancellationToken)
    {
        var subscription = await _context.Subscriptions
            .AsNoTracking()
            .Include(s => s.Client)
            .Include(s => s.Service)
            .Include(s => s.PriceHistory)
            .Include(s => s.ChangeLogs)
            .Include(s => s.TermsVersions)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (subscription is null)
        {
            throw new InvalidOperationException("La suscripción no existe.");
        }

        if (_currentUserService.IsClientUser())
        {
            if (!_currentUserService.ClientId.HasValue || _currentUserService.ClientId.Value != subscription.ClientId)
            {
                throw new InvalidOperationException("No tiene permiso para consultar esta suscripción.");
            }

            if (_currentUserService.UserId.HasValue)
            {
                var hasRestrictions = await _context.SubscriptionPermissions
                    .AsNoTracking()
                    .AnyAsync(sp => sp.UserId == _currentUserService.UserId.Value, cancellationToken);

                if (hasRestrictions)
                {
                    var isAllowed = await _context.SubscriptionPermissions
                        .AsNoTracking()
                        .AnyAsync(sp => sp.UserId == _currentUserService.UserId.Value && sp.SubscriptionId == subscription.Id, cancellationToken);

                    if (!isAllowed)
                    {
                        throw new InvalidOperationException("No tiene permiso para consultar esta suscripción.");
                    }
                }
            }
        }

        return new SubscriptionDetailDto
        {
            Id = subscription.Id,
            Code = subscription.Code,
            ClientId = subscription.ClientId,
            ClientName = subscription.Client.Name,
            ServiceId = subscription.ServiceId,
            ServiceName = subscription.Service.Name,
            BillingType = subscription.BillingType.ToString(),
            Price = subscription.Price,
            Currency = subscription.Currency,
            StartDate = subscription.StartDate,
            EndDate = subscription.EndDate,
            BillingDay = subscription.BillingDay,
            ServiceVersionId = subscription.ServiceVersionId,
            CustomIntervalDays = subscription.CustomIntervalDays,
            DiscountPercent = subscription.DiscountPercent,
            TaxPercent = subscription.TaxPercent,
            ProrationPolicy = subscription.ProrationPolicy.ToString(),
            ContractTerms = subscription.ContractTerms,
            TermsVersions = subscription.TermsVersions.OrderByDescending(t => t.EffectiveFrom).Select(t => new SubscriptionTermsVersionDto(
                t.Id, t.VersionNumber, t.ServiceVersionId, t.EffectiveFrom, t.EffectiveTo, t.BillingType.ToString(),
                t.BasePrice, t.Currency, t.DiscountPercent, t.TaxPercent, t.BillingDay, t.CustomIntervalDays,
                t.ProrationPolicy.ToString(), t.Terms, t.Reason,
                Domain.Accounting.SubscriptionPricingRules.Calculate(t.BasePrice, t.DiscountPercent, t.TaxPercent).Total)).ToList(),
            Status = subscription.Status.ToString(),
            AutoRenew = subscription.AutoRenew,
            Notes = subscription.Notes,
            LastBillingDate = subscription.LastBillingDate,
            NextBillingDate = subscription.NextBillingDate,
            PriceHistory = subscription.PriceHistory
                .OrderByDescending(h => h.EffectiveDate)
                .ThenByDescending(h => h.CreatedAt)
                .Select(h => new SubscriptionPriceHistoryDto
                {
                    Id = h.Id,
                    SubscriptionId = h.SubscriptionId,
                    OldPrice = h.OldPrice,
                    NewPrice = h.NewPrice,
                    EffectiveDate = h.EffectiveDate,
                    Reason = h.Reason,
                    CreatedAt = h.CreatedAt
                }).ToList(),
            ChangeLogs = subscription.ChangeLogs
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new SubscriptionChangeLogDto
                {
                    Id = c.Id,
                    SubscriptionId = c.SubscriptionId,
                    ChangeType = c.ChangeType.ToString(),
                    OldValue = c.OldValue,
                    NewValue = c.NewValue,
                    Reason = c.Reason,
                    CreatedAt = c.CreatedAt
                }).ToList()
        };
    }
}
