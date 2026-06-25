using MediatR;

namespace AegiFinance.Application.Features.Users.Commands.DeactivateUser;

public class DeactivateUserCommand : IRequest
{
    public Guid Id { get; set; }

    public DeactivateUserCommand() { }

    public DeactivateUserCommand(Guid id)
    {
        Id = id;
    }
}
