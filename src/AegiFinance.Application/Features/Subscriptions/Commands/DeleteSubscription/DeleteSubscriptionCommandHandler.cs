using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Subscriptions.Commands.DeleteSubscription;

public class DeleteSubscriptionCommandHandler : IRequestHandler<DeleteSubscriptionCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeleteSubscriptionCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(DeleteSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var subscription = await _context.Subscriptions
            .Include(s => s.PriceHistory)
            .Include(s => s.ChangeLogs)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (subscription is null)
        {
            throw new InvalidOperationException("La suscripción no existe.");
        }

        EnsureClientAccess(subscription.ClientId);

        if (subscription.PriceHistory.Count > 0 || subscription.ChangeLogs.Count > 0)
        {
            subscription.Status = SubscriptionStatus.Cancelled;
            subscription.IsDeleted = true;
            subscription.DeletedAt = DateTime.UtcNow;
        }
        else
        {
            _context.Subscriptions.Remove(subscription);
        }

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
