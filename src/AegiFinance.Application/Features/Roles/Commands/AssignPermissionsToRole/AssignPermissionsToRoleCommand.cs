using MediatR;

namespace AegiFinance.Application.Features.Roles.Commands.AssignPermissionsToRole;

public class AssignPermissionsToRoleCommand : IRequest
{
    public Guid RoleId { get; set; }
    public List<Guid> PermissionIds { get; set; } = new List<Guid>();
}
