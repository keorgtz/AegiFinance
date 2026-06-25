using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Enums;
using MediatR;

namespace AegiFinance.Application.Features.Roles.Commands.UpdateRole;

public class UpdateRoleCommand : IRequest<RoleDto>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public UserType? UserType { get; set; }
}
