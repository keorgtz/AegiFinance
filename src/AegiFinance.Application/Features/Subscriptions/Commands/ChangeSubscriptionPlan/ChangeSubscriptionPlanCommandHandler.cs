using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Domain.Accounting;
using AegiFinance.Domain.Entities;
using AegiFinance.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Subscriptions.Commands.ChangeSubscriptionPlan;

public sealed class ChangeSubscriptionPlanCommandHandler : IRequestHandler<ChangeSubscriptionPlanCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public ChangeSubscriptionPlanCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser) { _context = context; _currentUser = currentUser; }

    public async Task Handle(ChangeSubscriptionPlanCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.IsClientUser()) throw new UnauthorizedAccessException("No se permite cambiar el plan desde el portal.");
        var subscription = await _context.Subscriptions.Include(s => s.TermsVersions).Include(s => s.ChangeLogs)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException("La suscripción no existe.");
        if (subscription.LastBillingDate.HasValue && request.EffectiveDate <= subscription.LastBillingDate.Value)
            throw new InvalidOperationException("La nueva versión no puede reescribir un período ya facturado.");
        if (subscription.TermsVersions.Any(t => t.EffectiveFrom == request.EffectiveDate))
            throw new InvalidOperationException("Ya existe un cambio de condiciones en esa fecha efectiva.");
        var serviceVersion = await _context.ServiceVersions.Include(v => v.Service)
            .FirstOrDefaultAsync(v => v.Id == request.ServiceVersionId && v.IsPublished, cancellationToken)
            ?? throw new InvalidOperationException("La versión del plan no existe o no está publicada.");

        var terms = new SubscriptionTermsVersion
        {
            Id = Guid.NewGuid(), SubscriptionId = subscription.Id, Subscription = subscription,
            VersionNumber = subscription.TermsVersions.Select(t => t.VersionNumber).DefaultIfEmpty().Max() + 1,
            ServiceVersionId = serviceVersion.Id, ServiceVersion = serviceVersion, EffectiveFrom = request.EffectiveDate,
            BillingType = serviceVersion.BillingType, BasePrice = request.BasePrice ?? serviceVersion.BasePrice,
            Currency = serviceVersion.Currency, DiscountPercent = request.DiscountPercent ?? serviceVersion.DefaultDiscountPercent,
            TaxPercent = request.TaxPercent ?? serviceVersion.DefaultTaxPercent, BillingDay = request.BillingDay ?? subscription.BillingDay,
            CustomIntervalDays = request.CustomIntervalDays ?? serviceVersion.CustomIntervalDays,
            ProrationPolicy = request.ProrationPolicy ?? serviceVersion.ProrationPolicy,
            Terms = request.Terms ?? serviceVersion.Terms, Reason = request.Reason
        };
        subscription.TermsVersions.Add(terms);
        var ordered = subscription.TermsVersions.OrderBy(t => t.EffectiveFrom).ThenBy(t => t.VersionNumber).ToList();
        for (var index = 0; index < ordered.Count; index++) ordered[index].EffectiveTo = index + 1 < ordered.Count ? ordered[index + 1].EffectiveFrom : null;
        var oldServiceId = subscription.ServiceId;
        if (request.EffectiveDate <= DateTime.UtcNow)
        {
            var price = SubscriptionPricingRules.Calculate(terms.BasePrice, terms.DiscountPercent, terms.TaxPercent);
            subscription.ServiceId = serviceVersion.ServiceId; subscription.Service = serviceVersion.Service;
            subscription.ServiceVersionId = serviceVersion.Id; subscription.ServiceVersion = serviceVersion;
            subscription.BillingType = terms.BillingType; subscription.Price = price.Total; subscription.Currency = terms.Currency;
            subscription.DiscountPercent = terms.DiscountPercent; subscription.TaxPercent = terms.TaxPercent;
            subscription.BillingDay = terms.BillingDay; subscription.CustomIntervalDays = terms.CustomIntervalDays;
            subscription.ProrationPolicy = terms.ProrationPolicy; subscription.ContractTerms = terms.Terms;
        }
        subscription.ChangeLogs.Add(new SubscriptionChangeLog { Id = Guid.NewGuid(), SubscriptionId = subscription.Id, Subscription = subscription, ChangeType = SubscriptionChangeType.PlanChange, OldValue = oldServiceId.ToString(), NewValue = serviceVersion.ServiceId.ToString(), Reason = request.Reason });
        await _context.SaveChangesAsync(cancellationToken);
    }
}
