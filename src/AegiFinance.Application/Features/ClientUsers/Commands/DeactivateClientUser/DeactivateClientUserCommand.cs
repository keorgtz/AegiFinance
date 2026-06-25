using MediatR;

namespace AegiFinance.Application.Features.ClientUsers.Commands.DeactivateClientUser;

public class DeactivateClientUserCommand : IRequest
{
    public Guid Id { get; set; }

    public DeactivateClientUserCommand() { }

    public DeactivateClientUserCommand(Guid id)
    {
        Id = id;
    }
}
