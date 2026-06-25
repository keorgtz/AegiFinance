using AegiFinance.Application.Common.Interfaces;
using MediatR;

namespace AegiFinance.Application.Features.Auth.Commands.ChangePassword;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand>
{
    private readonly IAuthService _authService;

    public ChangePasswordCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public Task Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        return _authService.ChangePasswordAsync(request.UserId, request.CurrentPassword, request.NewPassword, cancellationToken);
    }
}
