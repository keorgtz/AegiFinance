using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Common.Models;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Entities;
using AegiFinance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AegiFinance.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly int _maxFailedAttempts;
    private readonly TimeSpan _lockoutDuration;
    private readonly TimeSpan _sessionLifetime;
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IPinHasher _pinHasher;
    private readonly ITokenService _tokenService;
    private readonly IPermissionService _permissionService;

    public AuthService(ApplicationDbContext context, IPasswordHasher passwordHasher, IPinHasher pinHasher,
        ITokenService tokenService, IPermissionService permissionService, IConfiguration configuration)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _pinHasher = pinHasher;
        _tokenService = tokenService;
        _permissionService = permissionService;
        _maxFailedAttempts = Math.Max(1, configuration.GetValue("Security:MaxFailedLoginAttempts", 5));
        _lockoutDuration = TimeSpan.FromMinutes(Math.Max(1, configuration.GetValue("Security:LockoutMinutes", 15)));
        _sessionLifetime = TimeSpan.FromDays(Math.Max(1, configuration.GetValue("Security:SessionLifetimeDays", 7)));
    }

    public async Task<AuthResult> LoginAsync(string usernameOrEmail, string password, AuthSessionContext session, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.Include(item => item.Roles)
            .FirstOrDefaultAsync(item => item.UserName == usernameOrEmail || item.Email == usernameOrEmail, cancellationToken);
        await EnsureCanAuthenticateAsync(user, user is not null && _passwordHasher.VerifyPassword(password, user.PasswordHash), cancellationToken);
        return await CreateSessionAsync(user!, session, cancellationToken);
    }

    public async Task<AuthResult> LoginWithPinAsync(string username, string pin, AuthSessionContext session, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.Include(item => item.Roles)
            .FirstOrDefaultAsync(item => item.UserName == username, cancellationToken);
        var credential = user is null ? null : await _context.ClientPinCredentials.AsNoTracking()
            .FirstOrDefaultAsync(item => item.UserId == user.Id, cancellationToken);
        var clientUserActive = user is null || !await _context.ClientUsers.AsNoTracking()
            .AnyAsync(item => item.UserId == user.Id && !item.IsActive, cancellationToken);
        var valid = credential is not null && _pinHasher.VerifyPin(pin, credential.PinHash) && clientUserActive;
        await EnsureCanAuthenticateAsync(user, valid, cancellationToken);
        return await CreateSessionAsync(user!, session, cancellationToken);
    }

    public async Task LogoutAsync(Guid userId, Guid? sessionId, CancellationToken cancellationToken = default)
    {
        var query = _context.UserSessions.Where(item => item.UserId == userId && item.RevokedAt == null);
        if (sessionId.HasValue) query = query.Where(item => item.Id == sessionId.Value);
        var sessions = await query.ToListAsync(cancellationToken);
        Revoke(sessions, userId, "User logout");
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<AuthResult> RefreshTokenAsync(string refreshToken, AuthSessionContext context, CancellationToken cancellationToken = default)
    {
        var selector = refreshToken.Split('.', 2)[0];
        if (!Guid.TryParseExact(selector, "N", out var sessionId))
            throw new UnauthorizedAccessException("Refresh token inválido o expirado.");
        var session = await _context.UserSessions.Include(item => item.User).ThenInclude(user => user.Roles)
            .FirstOrDefaultAsync(item => item.Id == sessionId && item.RevokedAt == null && item.ExpiresAt > DateTime.UtcNow && item.User.IsActive, cancellationToken);
        if (session is null || !_passwordHasher.VerifyPassword(refreshToken, session.RefreshTokenHash))
            throw new UnauthorizedAccessException("Refresh token inválido o expirado.");
        return await RotateSessionAsync(session, context, cancellationToken);
    }

    public async Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(item => item.Id == userId, cancellationToken)
            ?? throw new InvalidOperationException("Usuario no encontrado.");
        if (!_passwordHasher.VerifyPassword(currentPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("La contraseña actual es incorrecta.");

        user.PasswordHash = _passwordHasher.HashPassword(newPassword);
        user.PasswordChangedAt = DateTime.UtcNow;
        user.MustChangePassword = false;
        var sessions = await _context.UserSessions.Where(item => item.UserId == userId && item.RevokedAt == null).ToListAsync(cancellationToken);
        Revoke(sessions, userId, "Password changed");
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureCanAuthenticateAsync(User? user, bool credentialsValid, CancellationToken cancellationToken)
    {
        if (user?.LockoutEnd > DateTime.UtcNow)
            throw new UnauthorizedAccessException("La cuenta está temporalmente bloqueada. Intentá nuevamente más tarde.");
        if (user is null) throw new UnauthorizedAccessException("Credenciales inválidas.");
        if (!credentialsValid)
        {
            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= _maxFailedAttempts) user.LockoutEnd = DateTime.UtcNow.Add(_lockoutDuration);
            await _context.SaveChangesAsync(cancellationToken);
            throw new UnauthorizedAccessException("Credenciales inválidas.");
        }
        if (!user.IsActive) throw new UnauthorizedAccessException("La cuenta está desactivada.");
        user.FailedLoginAttempts = 0;
        user.LockoutEnd = null;
    }

    private async Task<AuthResult> CreateSessionAsync(User user, AuthSessionContext context, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var session = new UserSession
        {
            Id = Guid.NewGuid(), UserId = user.Id, LastSeenAt = now, ExpiresAt = now.Add(_sessionLifetime),
            IpAddress = Limit(context.IpAddress, 64), UserAgent = Limit(context.UserAgent, 500), DeviceName = Limit(context.DeviceName, 120)
        };
        _context.UserSessions.Add(session);
        user.LastLoginAt = now;
        return await IssueTokensAsync(user, session, cancellationToken);
    }

    private Task<AuthResult> RotateSessionAsync(UserSession session, AuthSessionContext context, CancellationToken cancellationToken)
    {
        session.LastSeenAt = DateTime.UtcNow;
        session.IpAddress = Limit(context.IpAddress, 64) ?? session.IpAddress;
        session.UserAgent = Limit(context.UserAgent, 500) ?? session.UserAgent;
        session.DeviceName = Limit(context.DeviceName, 120) ?? session.DeviceName;
        return IssueTokensAsync(session.User, session, cancellationToken);
    }

    private async Task<AuthResult> IssueTokensAsync(User user, UserSession session, CancellationToken cancellationToken)
    {
        var permissions = await _permissionService.GetEffectivePermissionsAsync(user.Id, cancellationToken);
        var refreshToken = $"{session.Id:N}.{_tokenService.GenerateRefreshToken()}";
        session.RefreshTokenHash = _tokenService.HashRefreshToken(refreshToken);
        session.ExpiresAt = DateTime.UtcNow.Add(_sessionLifetime);
        await _context.SaveChangesAsync(cancellationToken);

        return new AuthResult
        {
            AccessToken = _tokenService.GenerateAccessToken(user, permissions, session.Id),
            RefreshToken = refreshToken,
            RefreshTokenExpiry = session.ExpiresAt,
            User = new UserDto
            {
                Id = user.Id, UserName = user.UserName, Email = user.Email, Name = user.Name,
                UserType = user.UserType.ToString(), ClientId = user.ClientId, IsActive = user.IsActive,
                EmailConfirmed = user.EmailConfirmed, LastLoginAt = user.LastLoginAt, MustChangePassword = user.MustChangePassword,
                Roles = user.Roles.Select(role => role.Name).ToList(), Permissions = permissions.ToList()
            }
        };
    }

    private static void Revoke(IEnumerable<UserSession> sessions, Guid actorId, string reason)
    {
        foreach (var session in sessions)
        {
            session.RevokedAt = DateTime.UtcNow;
            session.RevokedBy = actorId;
            session.RevokedReason = reason;
        }
    }

    private static string? Limit(string? value, int length)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var trimmed = value.Trim();
        return trimmed[..Math.Min(trimmed.Length, length)];
    }
}
