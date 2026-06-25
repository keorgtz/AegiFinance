using MediatR;

namespace AegiFinance.Application.Features.Users.Commands.AssignRolesToUser;

public class AssignRolesToUserCommand : IRequest
{
    public Guid UserId { get; set; }
    public List<Guid> RoleIds { get; set; } = new List<Guid>();
}
