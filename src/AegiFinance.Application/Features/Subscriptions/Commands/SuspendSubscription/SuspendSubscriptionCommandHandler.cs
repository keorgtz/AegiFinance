using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Domain.Entities;
using AegiFinance.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Subscriptions.Commands.SuspendSubscription;

public class SuspendSubscriptionCommandHandler : IRequestHandler<SuspendSubscriptionCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public SuspendSubscriptionCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(SuspendSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (subscription is null)
        {
            throw new InvalidOperationException("La suscripción no existe.");
        }

        EnsureClientAccess(subscription.ClientId);

        if (subscription.Status == SubscriptionStatus.Cancelled || subscription.Status == SubscriptionStatus.Expired)
        {
            throw new InvalidOperationException("No se puede suspender una suscripción cancelada o expirada.");
        }

        var oldValue = subscription.Status.ToString();
        subscription.Status = SubscriptionStatus.Suspended;

        subscription.ChangeLogs.Add(new SubscriptionChangeLog
        {
            Id = Guid.NewGuid(),
            SubscriptionId = subscription.Id,
            ChangeType = SubscriptionChangeType.Suspend,
            OldValue = oldValue,
            NewValue = subscription.Status.ToString(),
            Reason = request.Reason
        });

        await _context.SaveChangesAsync(cancellationToken);
    }

    private void EnsureClientAccess(Guid clientId)
    {
        var isClientUser = Enum.TryParse<UserType>(_currentUserService.UserType, out var userType)
            && userType == UserType.Client;

        if (isClientUser && (!_currentUserService.ClientId.HasValue || _currentUserService.ClientId.Value != clientId))
        {
            throw new InvalidOperationException("No tiene permiso para modificar esta suscripción.");
        }
    }
}
