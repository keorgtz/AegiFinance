using AegiFinance.Application.Common.Models;
using MediatR;

namespace AegiFinance.Application.Features.Auth.Commands.Login;

public class LoginCommand : IRequest<AuthResult>
{
    public string UsernameOrEmail { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
