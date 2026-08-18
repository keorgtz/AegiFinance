using MediatR;

namespace AegiFinance.Application.Features.Auth.Commands.Logout;

public class LogoutCommand : IRequest
{
    public Guid UserId { get; set; }
    public Guid? SessionId { get; set; }

    public LogoutCommand() { }

    public LogoutCommand(Guid userId, Guid? sessionId = null)
    {
        UserId = userId;
        SessionId = sessionId;
    }
}
