using MediatR;

namespace AegiFinance.Application.Features.ClientUsers.Commands.ActivateClientUser;

public class ActivateClientUserCommand : IRequest
{
    public Guid Id { get; set; }

    public ActivateClientUserCommand() { }

    public ActivateClientUserCommand(Guid id)
    {
        Id = id;
    }
}
