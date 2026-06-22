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
        var subscription = await _context.Subscriptions
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (subscription is null)
        {
            throw new InvalidOperationException("La suscripción no existe.");
        }

        EnsureClientAccess(subscription.ClientId);

        var oldValue = subscription.Status.ToString();
        subscription.Status = SubscriptionStatus.Cancelled;

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
