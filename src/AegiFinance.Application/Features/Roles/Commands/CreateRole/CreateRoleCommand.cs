using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Enums;
using MediatR;

namespace AegiFinance.Application.Features.Roles.Commands.CreateRole;

public class CreateRoleCommand : IRequest<RoleDto>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public UserType? UserType { get; set; }
}
