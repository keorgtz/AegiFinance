using AegiFinance.Application.Common.Models;

namespace AegiFinance.Application.Common.Interfaces;

public interface IAuthService
{
    Task<AuthResult> LoginAsync(string usernameOrEmail, string password, CancellationToken cancellationToken = default);
    Task<AuthResult> LoginWithPinAsync(string username, string pin, CancellationToken cancellationToken = default);
    Task LogoutAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<AuthResult> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default);
}
