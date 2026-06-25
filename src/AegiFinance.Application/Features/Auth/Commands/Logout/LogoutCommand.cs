using MediatR;

namespace AegiFinance.Application.Features.Auth.Commands.Logout;

public class LogoutCommand : IRequest
{
    public Guid UserId { get; set; }

    public LogoutCommand() { }

    public LogoutCommand(Guid userId)
    {
        UserId = userId;
    }
}
