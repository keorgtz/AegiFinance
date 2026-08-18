using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Entities;
using AegiFinance.Domain.Enums;
using AegiFinance.Domain.Accounting;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Subscriptions.Commands.ChangeSubscriptionPrice;

public class ChangeSubscriptionPriceCommandHandler : IRequestHandler<ChangeSubscriptionPriceCommand, SubscriptionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public ChangeSubscriptionPriceCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<SubscriptionDto> Handle(ChangeSubscriptionPriceCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.IsClientUser())
        {
            throw new UnauthorizedAccessException("No se permite cambiar el precio de suscripciones desde el portal.");
        }

        var subscription = await _context.Subscriptions
            .Include(s => s.Client)
            .Include(s => s.Service)
            .Include(s => s.TermsVersions)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (subscription is null)
        {
            throw new InvalidOperationException("La suscripción no existe.");
        }
        if (subscription.LastBillingDate.HasValue && request.EffectiveDate <= subscription.LastBillingDate.Value)
            throw new InvalidOperationException("El precio no puede cambiar un período ya facturado.");
        if (subscription.TermsVersions.Any(t => t.EffectiveFrom == request.EffectiveDate))
            throw new InvalidOperationException("Ya existe un cambio de condiciones en esa fecha efectiva.");

        var previousTerms = SubscriptionPricingRules.ResolveTerms(subscription.TermsVersions, request.EffectiveDate);
        var oldPrice = SubscriptionPricingRules.Calculate(previousTerms.BasePrice, previousTerms.DiscountPercent, previousTerms.TaxPercent).Total;
        var newPrice = SubscriptionPricingRules.Calculate(request.NewPrice, previousTerms.DiscountPercent, previousTerms.TaxPercent).Total;
        var newTerms = new SubscriptionTermsVersion
        {
            Id = Guid.NewGuid(), SubscriptionId = subscription.Id, Subscription = subscription,
            VersionNumber = subscription.TermsVersions.Select(t => t.VersionNumber).DefaultIfEmpty().Max() + 1,
            ServiceVersionId = previousTerms.ServiceVersionId, EffectiveFrom = request.EffectiveDate,
            BillingType = previousTerms.BillingType, BasePrice = request.NewPrice, Currency = previousTerms.Currency,
            DiscountPercent = previousTerms.DiscountPercent, TaxPercent = previousTerms.TaxPercent,
            BillingDay = previousTerms.BillingDay, CustomIntervalDays = previousTerms.CustomIntervalDays,
            ProrationPolicy = previousTerms.ProrationPolicy, Terms = previousTerms.Terms, Reason = request.Reason
        };
        subscription.TermsVersions.Add(newTerms);
        var orderedTerms = subscription.TermsVersions.OrderBy(t => t.EffectiveFrom).ThenBy(t => t.VersionNumber).ToList();
        for (var index = 0; index < orderedTerms.Count; index++)
            orderedTerms[index].EffectiveTo = index + 1 < orderedTerms.Count ? orderedTerms[index + 1].EffectiveFrom : null;
        if (request.EffectiveDate <= DateTime.UtcNow)
            subscription.Price = newPrice;

        subscription.PriceHistory.Add(new SubscriptionPriceHistory
        {
            Id = Guid.NewGuid(),
            SubscriptionId = subscription.Id,
            Subscription = subscription,
            OldPrice = oldPrice,
            NewPrice = newPrice,
            EffectiveDate = request.EffectiveDate,
            Reason = request.Reason
        });

        subscription.ChangeLogs.Add(new SubscriptionChangeLog
        {
            Id = Guid.NewGuid(),
            SubscriptionId = subscription.Id,
            ChangeType = SubscriptionChangeType.PriceChange,
            OldValue = oldPrice.ToString("F2"),
            NewValue = newPrice.ToString("F2"),
            Reason = request.Reason
        });

        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(subscription);
    }

    private static SubscriptionDto MapToDto(Subscription subscription)
    {
        return new SubscriptionDto
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
            Status = subscription.Status.ToString(),
            AutoRenew = subscription.AutoRenew,
            Notes = subscription.Notes,
            LastBillingDate = subscription.LastBillingDate,
            NextBillingDate = subscription.NextBillingDate
        };
    }
}
