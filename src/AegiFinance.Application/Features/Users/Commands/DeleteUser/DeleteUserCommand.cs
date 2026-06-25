using MediatR;

namespace AegiFinance.Application.Features.Users.Commands.DeleteUser;

public class DeleteUserCommand : IRequest
{
    public Guid Id { get; set; }

    public DeleteUserCommand() { }

    public DeleteUserCommand(Guid id)
    {
        Id = id;
    }
}
