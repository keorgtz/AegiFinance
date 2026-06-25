using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Helpers;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Domain.Entities;
using AegiFinance.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Subscriptions.Commands.ReactivateSubscription;

public class ReactivateSubscriptionCommandHandler : IRequestHandler<ReactivateSubscriptionCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public ReactivateSubscriptionCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(ReactivateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.IsClientUser())
        {
            throw new UnauthorizedAccessException("No se permite reactivar suscripciones desde el portal.");
        }

        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (subscription is null)
        {
            throw new InvalidOperationException("La suscripción no existe.");
        }

        if (subscription.Status == SubscriptionStatus.Cancelled || subscription.Status == SubscriptionStatus.Expired)
        {
            throw new InvalidOperationException("No se puede reactivar una suscripción cancelada o expirada.");
        }

        var oldValue = subscription.Status.ToString();
        subscription.Status = SubscriptionStatus.Active;

        if (subscription.BillingType == BillingType.Monthly || subscription.BillingType == BillingType.Yearly)
        {
            var today = DateTime.UtcNow.Date;
            var baseDate = subscription.NextBillingDate ?? subscription.StartDate;

            while (!subscription.NextBillingDate.HasValue || subscription.NextBillingDate.Value.Date < today)
            {
                baseDate = SubscriptionDateCalculator.CalculateNextBillingDate(baseDate, subscription.BillingType, subscription.BillingDay);
                subscription.NextBillingDate = baseDate;
            }
        }

        subscription.ChangeLogs.Add(new SubscriptionChangeLog
        {
            Id = Guid.NewGuid(),
            SubscriptionId = subscription.Id,
            ChangeType = SubscriptionChangeType.Reactivate,
            OldValue = oldValue,
            NewValue = subscription.Status.ToString(),
            Reason = request.Reason
        });

        await _context.SaveChangesAsync(cancellationToken);
    }
}
