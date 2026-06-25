using MediatR;

namespace AegiFinance.Application.Features.ClientUsers.Commands.DeleteClientUser;

public class DeleteClientUserCommand : IRequest
{
    public Guid Id { get; set; }

    public DeleteClientUserCommand() { }

    public DeleteClientUserCommand(Guid id)
    {
        Id = id;
    }
}
