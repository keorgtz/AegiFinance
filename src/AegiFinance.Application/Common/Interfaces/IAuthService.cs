using AegiFinance.Application.Common.Models;

namespace AegiFinance.Application.Common.Interfaces;

public interface IAuthService
{
    Task<AuthResult> LoginAsync(string usernameOrEmail, string password, AuthSessionContext session, CancellationToken cancellationToken = default);
    Task<AuthResult> LoginWithPinAsync(string username, string pin, AuthSessionContext session, CancellationToken cancellationToken = default);
    Task LogoutAsync(Guid userId, Guid? sessionId, CancellationToken cancellationToken = default);
    Task<AuthResult> RefreshTokenAsync(string refreshToken, AuthSessionContext session, CancellationToken cancellationToken = default);
    Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default);
}
