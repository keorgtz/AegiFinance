using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.SubscriptionPermissions.Queries.GetSubscriptionPermissions;

public class GetSubscriptionPermissionsQueryHandler : IRequestHandler<GetSubscriptionPermissionsQuery, List<SubscriptionPermissionDto>>
{
    private readonly IApplicationDbContext _context;

    public GetSubscriptionPermissionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SubscriptionPermissionDto>> Handle(GetSubscriptionPermissionsQuery request, CancellationToken cancellationToken)
    {
        var subscriptionPermissions = await _context.SubscriptionPermissions
            .AsNoTracking()
            .Where(sp => sp.UserId == request.UserId)
            .ToListAsync(cancellationToken);

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        var subscriptionIds = subscriptionPermissions.Select(sp => sp.SubscriptionId).ToList();

        var subscriptions = await _context.Subscriptions
            .AsNoTracking()
            .Include(s => s.Service)
            .Where(s => subscriptionIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, cancellationToken);

        return subscriptionPermissions.Select(sp => new SubscriptionPermissionDto
        {
            Id = sp.Id,
            UserId = sp.UserId,
            UserName = user?.Name ?? string.Empty,
            SubscriptionId = sp.SubscriptionId,
            SubscriptionCode = subscriptions.TryGetValue(sp.SubscriptionId, out var subscription) ? subscription.Code : string.Empty,
            ServiceName = subscriptions.TryGetValue(sp.SubscriptionId, out var subscription2) ? subscription2.Service.Name : string.Empty
        }).ToList();
    }
}
