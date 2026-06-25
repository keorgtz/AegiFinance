using MediatR;

namespace AegiFinance.Application.Features.Permissions.Commands.DeletePermission;

public class DeletePermissionCommand : IRequest
{
    public Guid Id { get; set; }

    public DeletePermissionCommand() { }

    public DeletePermissionCommand(Guid id)
    {
        Id = id;
    }
}
