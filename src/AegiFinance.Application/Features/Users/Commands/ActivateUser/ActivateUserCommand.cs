using MediatR;

namespace AegiFinance.Application.Features.Users.Commands.ActivateUser;

public class ActivateUserCommand : IRequest
{
    public Guid Id { get; set; }

    public ActivateUserCommand() { }

    public ActivateUserCommand(Guid id)
    {
        Id = id;
    }
}
