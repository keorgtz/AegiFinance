using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Helpers;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Domain.Entities;
using AegiFinance.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Subscriptions.Commands.RenewSubscription;

public class RenewSubscriptionCommandHandler : IRequestHandler<RenewSubscriptionCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public RenewSubscriptionCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(RenewSubscriptionCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.IsClientUser())
        {
            throw new UnauthorizedAccessException("No se permite renovar suscripciones desde el portal.");
        }

        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (subscription is null)
        {
            throw new InvalidOperationException("La suscripción no existe.");
        }

        if (subscription.Status == SubscriptionStatus.Cancelled)
        {
            throw new InvalidOperationException("No se puede renovar una suscripción cancelada.");
        }

        var baseDate = subscription.NextBillingDate ?? subscription.StartDate;
        subscription.LastBillingDate = baseDate;

        if (subscription.BillingType == BillingType.Monthly || subscription.BillingType == BillingType.Yearly)
        {
            subscription.NextBillingDate = SubscriptionDateCalculator.CalculateNextBillingDate(
                baseDate,
                subscription.BillingType,
                subscription.BillingDay);

            if (subscription.EndDate.HasValue)
            {
                subscription.EndDate = subscription.BillingType == BillingType.Yearly
                    ? subscription.EndDate.Value.AddYears(1)
                    : subscription.EndDate.Value.AddMonths(1);
            }
        }

        if (subscription.Status == SubscriptionStatus.Expired || subscription.Status == SubscriptionStatus.Suspended)
        {
            subscription.Status = SubscriptionStatus.Active;
        }

        subscription.ChangeLogs.Add(new SubscriptionChangeLog
        {
            Id = Guid.NewGuid(),
            SubscriptionId = subscription.Id,
            ChangeType = SubscriptionChangeType.Renew,
            OldValue = baseDate.ToString("O"),
            NewValue = subscription.NextBillingDate?.ToString("O"),
            Reason = request.Reason
        });

        await _context.SaveChangesAsync(cancellationToken);
    }
}
