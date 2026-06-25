using AegiFinance.Application.Common.Interfaces;
using MediatR;

namespace AegiFinance.Application.Features.Auth.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly IAuthService _authService;

    public LogoutCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        return _authService.LogoutAsync(request.UserId, cancellationToken);
    }
}
