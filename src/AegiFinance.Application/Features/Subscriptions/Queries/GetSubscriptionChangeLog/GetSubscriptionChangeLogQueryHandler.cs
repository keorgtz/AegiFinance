using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Subscriptions.Queries.GetSubscriptionChangeLog;

public class GetSubscriptionChangeLogQueryHandler : IRequestHandler<GetSubscriptionChangeLogQuery, List<SubscriptionChangeLogDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetSubscriptionChangeLogQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<SubscriptionChangeLogDto>> Handle(GetSubscriptionChangeLogQuery request, CancellationToken cancellationToken)
    {
        var subscription = await _context.Subscriptions
            .AsNoTracking()
            .Select(s => new { s.Id, s.ClientId })
            .FirstOrDefaultAsync(s => s.Id == request.SubscriptionId, cancellationToken);

        if (subscription is null)
        {
            throw new InvalidOperationException("La suscripción no existe.");
        }

        await EnsureAccessAsync(subscription.Id, subscription.ClientId, cancellationToken);

        return await _context.SubscriptionChangeLogs
            .AsNoTracking()
            .Where(c => c.SubscriptionId == request.SubscriptionId)
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
            })
            .ToListAsync(cancellationToken);
    }

    private async Task EnsureAccessAsync(Guid subscriptionId, Guid clientId, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsClientUser())
        {
            return;
        }

        if (!_currentUserService.ClientId.HasValue || _currentUserService.ClientId.Value != clientId)
        {
            throw new InvalidOperationException("No tiene permiso para consultar esta suscripción.");
        }

        if (!_currentUserService.UserId.HasValue)
        {
            return;
        }

        var hasRestrictions = await _context.SubscriptionPermissions
            .AsNoTracking()
            .AnyAsync(sp => sp.UserId == _currentUserService.UserId.Value, cancellationToken);

        if (!hasRestrictions)
        {
            return;
        }

        var isAllowed = await _context.SubscriptionPermissions
            .AsNoTracking()
            .AnyAsync(sp => sp.UserId == _currentUserService.UserId.Value && sp.SubscriptionId == subscriptionId, cancellationToken);

        if (!isAllowed)
        {
            throw new InvalidOperationException("No tiene permiso para consultar esta suscripción.");
        }
    }
}
