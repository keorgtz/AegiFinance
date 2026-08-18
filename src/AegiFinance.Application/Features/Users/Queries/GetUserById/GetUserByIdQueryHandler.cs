using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Users.Queries.GetUserById;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IPermissionService _permissionService;

    public GetUserByIdQueryHandler(IApplicationDbContext context, IPermissionService permissionService)
    {
        _context = context;
        _permissionService = permissionService;
    }

    public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (user is null)
        {
            return null;
        }

        var permissions = await _permissionService.GetEffectivePermissionsAsync(user.Id, cancellationToken);

        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Name = user.Name,
            UserType = user.UserType.ToString(),
            ClientId = user.ClientId,
            IsActive = user.IsActive,
            EmailConfirmed = user.EmailConfirmed,
            LastLoginAt = user.LastLoginAt,
            LockoutEnd = user.LockoutEnd,
            MustChangePassword = user.MustChangePassword,
            ActiveSessionCount = await _context.UserSessions.CountAsync(session => session.UserId == user.Id && session.RevokedAt == null && session.ExpiresAt > DateTime.UtcNow, cancellationToken),
            Roles = user.Roles.Select(r => r.Name).ToList(),
            Permissions = permissions.ToList()
        };
    }
}
