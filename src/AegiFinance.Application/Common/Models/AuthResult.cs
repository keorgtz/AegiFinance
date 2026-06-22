using AegiFinance.Application.Dtos;

namespace AegiFinance.Application.Common.Models;

public class AuthResult
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpiry { get; set; }
    public UserDto User { get; set; } = new UserDto();
}
