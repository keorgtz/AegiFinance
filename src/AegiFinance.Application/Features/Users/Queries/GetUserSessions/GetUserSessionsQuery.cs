using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Users.Queries.GetUserSessions;

public sealed record GetUserSessionsQuery(Guid UserId) : IRequest<List<UserSessionDto>>;

public sealed class GetUserSessionsQueryHandler : IRequestHandler<GetUserSessionsQuery, List<UserSessionDto>>
{
    private readonly IApplicationDbContext _context;
    public GetUserSessionsQueryHandler(IApplicationDbContext context) => _context = context;

    public Task<List<UserSessionDto>> Handle(GetUserSessionsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        return _context.UserSessions.AsNoTracking().Where(item => item.UserId == request.UserId)
            .OrderByDescending(item => item.LastSeenAt).Take(50)
            .Select(item => new UserSessionDto
            {
                Id = item.Id, DeviceName = item.DeviceName ?? "Dispositivo desconocido", IpAddress = item.IpAddress,
                LastSeenAt = item.LastSeenAt, ExpiresAt = item.ExpiresAt, RevokedAt = item.RevokedAt,
                IsActive = item.RevokedAt == null && item.ExpiresAt > now
            }).ToListAsync(cancellationToken);
    }
}
