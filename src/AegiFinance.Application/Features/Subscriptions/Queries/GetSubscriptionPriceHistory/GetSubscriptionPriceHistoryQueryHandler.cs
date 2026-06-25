using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Subscriptions.Queries.GetSubscriptionPriceHistory;

public class GetSubscriptionPriceHistoryQueryHandler : IRequestHandler<GetSubscriptionPriceHistoryQuery, List<SubscriptionPriceHistoryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetSubscriptionPriceHistoryQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<SubscriptionPriceHistoryDto>> Handle(GetSubscriptionPriceHistoryQuery request, CancellationToken cancellationToken)
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

        return await _context.SubscriptionPriceHistories
            .AsNoTracking()
            .Where(h => h.SubscriptionId == request.SubscriptionId)
            .OrderByDescending(h => h.EffectiveDate)
            .ThenByDescending(h => h.CreatedAt)
            .Select(h => new SubscriptionPriceHistoryDto
            {
                Id = h.Id,
                SubscriptionId = h.SubscriptionId,
                OldPrice = h.OldPrice,
                NewPrice = h.NewPrice,
                EffectiveDate = h.EffectiveDate,
                Reason = h.Reason,
                CreatedAt = h.CreatedAt
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
