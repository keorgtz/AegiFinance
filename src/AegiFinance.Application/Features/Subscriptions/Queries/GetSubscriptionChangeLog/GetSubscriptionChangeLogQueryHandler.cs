using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Enums;
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

        EnsureClientAccess(subscription.ClientId);

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
