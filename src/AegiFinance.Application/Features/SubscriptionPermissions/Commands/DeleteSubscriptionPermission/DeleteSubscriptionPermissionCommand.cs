using MediatR;

namespace AegiFinance.Application.Features.SubscriptionPermissions.Commands.DeleteSubscriptionPermission;

public class DeleteSubscriptionPermissionCommand : IRequest
{
    public Guid Id { get; set; }

    public DeleteSubscriptionPermissionCommand() { }

    public DeleteSubscriptionPermissionCommand(Guid id)
    {
        Id = id;
    }
}
