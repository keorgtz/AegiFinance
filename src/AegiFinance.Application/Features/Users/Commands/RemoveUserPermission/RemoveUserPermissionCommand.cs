using MediatR;

namespace AegiFinance.Application.Features.Users.Commands.RemoveUserPermission;

public class RemoveUserPermissionCommand : IRequest
{
    public Guid UserId { get; set; }
    public Guid PermissionId { get; set; }
    public Guid? ClientId { get; set; }
    public Guid? SubscriptionId { get; set; }

    public RemoveUserPermissionCommand() { }

    public RemoveUserPermissionCommand(Guid userId, Guid permissionId)
    {
        UserId = userId;
        PermissionId = permissionId;
    }
}
