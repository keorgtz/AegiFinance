using AegiFinance.Application.Common.Models;
using MediatR;

namespace AegiFinance.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommand : IRequest<AuthResult>
{
    public string RefreshToken { get; set; } = string.Empty;

    public RefreshTokenCommand() { }

    public RefreshTokenCommand(string refreshToken)
    {
        RefreshToken = refreshToken;
    }
}
