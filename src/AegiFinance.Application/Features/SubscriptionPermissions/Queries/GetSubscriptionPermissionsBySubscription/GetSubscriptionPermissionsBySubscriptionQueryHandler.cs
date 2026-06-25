using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.SubscriptionPermissions.Queries.GetSubscriptionPermissionsBySubscription;

public class GetSubscriptionPermissionsBySubscriptionQueryHandler : IRequestHandler<GetSubscriptionPermissionsBySubscriptionQuery, List<SubscriptionPermissionDto>>
{
    private readonly IApplicationDbContext _context;

    public GetSubscriptionPermissionsBySubscriptionQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SubscriptionPermissionDto>> Handle(GetSubscriptionPermissionsBySubscriptionQuery request, CancellationToken cancellationToken)
    {
        var subscription = await _context.Subscriptions
            .AsNoTracking()
            .Include(s => s.Service)
            .FirstOrDefaultAsync(s => s.Id == request.SubscriptionId, cancellationToken);

        if (subscription is null)
        {
            throw new InvalidOperationException("La suscripción no existe.");
        }

        var subscriptionPermissions = await _context.SubscriptionPermissions
            .AsNoTracking()
            .Where(sp => sp.SubscriptionId == request.SubscriptionId)
            .ToListAsync(cancellationToken);

        var userIds = subscriptionPermissions.Select(sp => sp.UserId).ToList();

        var users = await _context.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, cancellationToken);

        return subscriptionPermissions.Select(sp => new SubscriptionPermissionDto
        {
            Id = sp.Id,
            UserId = sp.UserId,
            UserName = users.TryGetValue(sp.UserId, out var user) ? user.Name : string.Empty,
            SubscriptionId = sp.SubscriptionId,
            SubscriptionCode = subscription.Code,
            ServiceName = subscription.Service.Name
        }).ToList();
    }
}
