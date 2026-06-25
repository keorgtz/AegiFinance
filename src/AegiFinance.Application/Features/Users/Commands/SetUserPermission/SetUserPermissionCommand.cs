using MediatR;

namespace AegiFinance.Application.Features.Users.Commands.SetUserPermission;

public class SetUserPermissionCommand : IRequest
{
    public Guid UserId { get; set; }
    public Guid PermissionId { get; set; }
    public bool IsGranted { get; set; }
}
