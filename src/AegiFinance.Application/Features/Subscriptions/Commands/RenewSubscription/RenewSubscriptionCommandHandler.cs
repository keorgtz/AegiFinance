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

        if (string.IsNullOrWhiteSpace(request.IdempotencyKey))
            throw new InvalidOperationException("La renovación requiere una clave de idempotencia.");
        if (await _context.SubscriptionRenewals.AnyAsync(r => r.SubscriptionId == request.Id && r.IdempotencyKey == request.IdempotencyKey, cancellationToken))
            return;

        await using var transaction = await _context.BeginTransactionAsync(cancellationToken);
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
        var previousEndDate = subscription.EndDate;
        var previousNextBillingDate = subscription.NextBillingDate;
        subscription.LastBillingDate = baseDate;

        if (subscription.BillingType is BillingType.Monthly or BillingType.Yearly or BillingType.Custom)
        {
            subscription.NextBillingDate = SubscriptionDateCalculator.CalculateNextBillingDate(
                baseDate,
                subscription.BillingType,
                subscription.BillingDay,
                subscription.CustomIntervalDays);

            if (subscription.EndDate.HasValue)
            {
                subscription.EndDate = subscription.BillingType == BillingType.Yearly
                    ? subscription.EndDate.Value.AddYears(1)
                    : subscription.BillingType == BillingType.Custom
                        ? subscription.EndDate.Value.AddDays(subscription.CustomIntervalDays!.Value)
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

        subscription.Renewals.Add(new SubscriptionRenewal
        {
            Id = Guid.NewGuid(), SubscriptionId = subscription.Id, Subscription = subscription,
            IdempotencyKey = request.IdempotencyKey.Trim(), PreviousEndDate = previousEndDate,
            NewEndDate = subscription.EndDate, PreviousNextBillingDate = previousNextBillingDate,
            NewNextBillingDate = subscription.NextBillingDate, RenewedAt = DateTime.UtcNow, Reason = request.Reason
        });

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
