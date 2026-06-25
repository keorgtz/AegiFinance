using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Common.Models;
using MediatR;

namespace AegiFinance.Application.Features.Auth.Commands.LoginWithPin;

public class LoginWithPinCommandHandler : IRequestHandler<LoginWithPinCommand, AuthResult>
{
    private readonly IAuthService _authService;

    public LoginWithPinCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public Task<AuthResult> Handle(LoginWithPinCommand request, CancellationToken cancellationToken)
    {
        return _authService.LoginWithPinAsync(request.UserName, request.Pin, cancellationToken);
    }
}
