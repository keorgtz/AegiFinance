using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Auth.Queries.GetCurrentUser;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, UserDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPermissionService _permissionService;
    private readonly IUiPermissionService _uiPermissionService;

    public GetCurrentUserQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IPermissionService permissionService,
        IUiPermissionService uiPermissionService)
    {
        _context = context;
        _currentUserService = currentUserService;
        _permissionService = permissionService;
        _uiPermissionService = uiPermissionService;
    }

    public async Task<UserDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.UserId is not Guid userId)
        {
            throw new UnauthorizedAccessException("No hay una sesión activa.");
        }

        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            throw new UnauthorizedAccessException("El usuario de la sesión ya no existe.");
        }

        var permissions = await _permissionService.GetEffectivePermissionsAsync(user.Id, cancellationToken);
        var uiPolicies = await _uiPermissionService.GetEffectivePoliciesAsync(user.Id, user.ClientId, null, cancellationToken);

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
            Permissions = permissions.ToList(),
            UiPolicies = new Dictionary<string, string>(uiPolicies, StringComparer.OrdinalIgnoreCase)
        };
    }
}
