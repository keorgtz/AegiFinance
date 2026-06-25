using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Common.Models;
using MediatR;

namespace AegiFinance.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResult>
{
    private readonly IAuthService _authService;

    public LoginCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public Task<AuthResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        return _authService.LoginAsync(request.UsernameOrEmail, request.Password, cancellationToken);
    }
}
