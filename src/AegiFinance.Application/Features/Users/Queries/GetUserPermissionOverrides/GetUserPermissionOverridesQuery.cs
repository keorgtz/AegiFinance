using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Users.Queries.GetUserPermissionOverrides;

public sealed record GetUserPermissionOverridesQuery(Guid UserId) : IRequest<List<UserPermissionOverrideDto>>;

public sealed class GetUserPermissionOverridesQueryHandler : IRequestHandler<GetUserPermissionOverridesQuery, List<UserPermissionOverrideDto>>
{
    private readonly IApplicationDbContext _context;
    public GetUserPermissionOverridesQueryHandler(IApplicationDbContext context) => _context = context;

    public Task<List<UserPermissionOverrideDto>> Handle(GetUserPermissionOverridesQuery request, CancellationToken cancellationToken) =>
        _context.UserPermissionOverrides.AsNoTracking().Where(item => item.UserId == request.UserId)
            .OrderBy(item => item.Permission.Module).ThenBy(item => item.Permission.Code)
            .Select(item => new UserPermissionOverrideDto
            {
                Id = item.Id, PermissionId = item.PermissionId, PermissionCode = item.Permission.Code,
                PermissionName = item.Permission.Name, IsGranted = item.IsGranted, ClientId = item.ClientId,
                SubscriptionId = item.SubscriptionId, ExpiresAt = item.ExpiresAt
            }).ToListAsync(cancellationToken);
}
