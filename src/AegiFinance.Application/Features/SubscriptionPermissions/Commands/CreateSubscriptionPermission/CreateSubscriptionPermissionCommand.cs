using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.SubscriptionPermissions.Commands.CreateSubscriptionPermission;

public class CreateSubscriptionPermissionCommand : IRequest<SubscriptionPermissionDto>
{
    public Guid UserId { get; set; }
    public Guid SubscriptionId { get; set; }
}
