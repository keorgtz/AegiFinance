using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Common.Models;
using MediatR;

namespace AegiFinance.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResult>
{
    private readonly IAuthService _authService;

    public RefreshTokenCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public Task<AuthResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        return _authService.RefreshTokenAsync(request.RefreshToken,
            new AuthSessionContext(request.IpAddress, request.UserAgent, request.DeviceName), cancellationToken);
    }
}
