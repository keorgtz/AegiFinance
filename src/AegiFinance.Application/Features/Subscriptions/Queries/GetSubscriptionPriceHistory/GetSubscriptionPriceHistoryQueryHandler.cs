using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Enums;
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

        EnsureClientAccess(subscription.ClientId);

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

    private void EnsureClientAccess(Guid clientId)
    {
        var isClientUser = Enum.TryParse<UserType>(_currentUserService.UserType, out var userType)
            && userType == UserType.Client;

        if (isClientUser && (!_currentUserService.ClientId.HasValue || _currentUserService.ClientId.Value != clientId))
        {
            throw new InvalidOperationException("No tiene permiso para consultar esta suscripción.");
        }
    }
}
