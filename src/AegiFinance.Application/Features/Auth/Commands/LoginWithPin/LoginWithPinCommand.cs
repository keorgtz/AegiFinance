using AegiFinance.Application.Common.Models;
using MediatR;

namespace AegiFinance.Application.Features.Auth.Commands.LoginWithPin;

public class LoginWithPinCommand : IRequest<AuthResult>
{
    public string UserName { get; set; } = string.Empty;
    public string Pin { get; set; } = string.Empty;
}
