using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Common.Models;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Entities;
using AegiFinance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IPinHasher _pinHasher;
    private readonly ITokenService _tokenService;
    private readonly IPermissionService _permissionService;

    public AuthService(
        ApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IPinHasher pinHasher,
        ITokenService tokenService,
        IPermissionService permissionService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _pinHasher = pinHasher;
        _tokenService = tokenService;
        _permissionService = permissionService;
    }

    public async Task<AuthResult> LoginAsync(string usernameOrEmail, string password, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.UserName == usernameOrEmail || u.Email == usernameOrEmail, cancellationToken);

        if (user is null || !_passwordHasher.VerifyPassword(password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Credenciales inválidas.");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("La cuenta está desactivada.");
        }

        return await BuildAuthResultAsync(user, cancellationToken);
    }

    public async Task<AuthResult> LoginWithPinAsync(string username, string pin, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserName == username, cancellationToken);

        if (user is null)
        {
            throw new UnauthorizedAccessException("Credenciales inválidas.");
        }

        var pinCredential = await _context.ClientPinCredentials
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == user.Id, cancellationToken);

        if (pinCredential is null || !_pinHasher.VerifyPin(pin, pinCredential.PinHash))
        {
            throw new UnauthorizedAccessException("Credenciales inválidas.");
        }

        return await BuildAuthResultAsync(user, cancellationToken);
    }

    public async Task LogoutAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is not null)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<AuthResult> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var users = await _context.Users
            .AsNoTracking()
            .Where(u => u.RefreshToken != null && u.RefreshTokenExpiry > DateTime.UtcNow)
            .Include(u => u.Roles)
            .ToListAsync(cancellationToken);

        var user = users.FirstOrDefault(u => _passwordHasher.VerifyPassword(refreshToken, u.RefreshToken!));

        if (user is null)
        {
            throw new UnauthorizedAccessException("Refresh token inválido o expirado.");
        }

        return await BuildAuthResultAsync(user, cancellationToken);
    }

    public async Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException("Usuario no encontrado.");
        }

        if (!_passwordHasher.VerifyPassword(currentPassword, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("La contraseña actual es incorrecta.");
        }

        user.PasswordHash = _passwordHasher.HashPassword(newPassword);
        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<AuthResult> BuildAuthResultAsync(User user, CancellationToken cancellationToken)
    {
        var permissions = await _permissionService.GetEffectivePermissionsAsync(user.Id, cancellationToken);
        var accessToken = _tokenService.GenerateAccessToken(user, permissions);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        var trackedUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == user.Id, cancellationToken);

        if (trackedUser is not null)
        {
            trackedUser.RefreshToken = _tokenService.HashRefreshToken(refreshToken);
            trackedUser.RefreshTokenExpiry = refreshTokenExpiry;
            trackedUser.LastLoginAt = DateTime.UtcNow;
            trackedUser.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }

        return new AuthResult
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            RefreshTokenExpiry = refreshTokenExpiry,
            User = new UserDto
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
                Roles = user.Roles.Select(r => r.Name).ToList(),
                Permissions = permissions.ToList()
            }
        };
    }
}
