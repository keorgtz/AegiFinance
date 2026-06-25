using MediatR;

namespace AegiFinance.Application.Features.Roles.Commands.DeleteRole;

public class DeleteRoleCommand : IRequest
{
    public Guid Id { get; set; }

    public DeleteRoleCommand() { }

    public DeleteRoleCommand(Guid id)
    {
        Id = id;
    }
}
