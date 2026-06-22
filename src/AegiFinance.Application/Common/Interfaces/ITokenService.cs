using AegiFinance.Domain.Entities;

namespace AegiFinance.Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user, IEnumerable<string> permissions);
    string GenerateRefreshToken();
    string HashRefreshToken(string refreshToken);
}
