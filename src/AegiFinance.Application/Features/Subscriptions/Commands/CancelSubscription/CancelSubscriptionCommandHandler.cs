using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Domain.Entities;
using AegiFinance.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Subscriptions.Commands.CancelSubscription;

public class CancelSubscriptionCommandHandler : IRequestHandler<CancelSubscriptionCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CancelSubscriptionCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(CancelSubscriptionCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.IsClientUser())
        {
            throw new UnauthorizedAccessException("No se permite cancelar suscripciones desde el portal.");
        }

        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (subscription is null)
        {
            throw new InvalidOperationException("La suscripción no existe.");
        }

        var oldValue = subscription.Status.ToString();
        subscription.Status = SubscriptionStatus.Cancelled;
        subscription.EndDate = request.EffectiveDate ?? DateTime.UtcNow.Date;
        subscription.NextBillingDate = null;

        subscription.ChangeLogs.Add(new SubscriptionChangeLog
        {
            Id = Guid.NewGuid(),
            SubscriptionId = subscription.Id,
            ChangeType = SubscriptionChangeType.Cancel,
            OldValue = oldValue,
            NewValue = subscription.Status.ToString(),
            Reason = request.Reason
        });

        await _context.SaveChangesAsync(cancellationToken);
    }
}
